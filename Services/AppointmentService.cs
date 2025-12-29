using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;

        public AppointmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. RANDEVULARI LİSTELEME
        public async Task<AppointmentListViewModel> GetPatientAppointmentsAsync(int patientId)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Department)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AppointmentDate)
                .AsNoTracking()
                .ToListAsync();

            return new AppointmentListViewModel
            {
                // AKTİF RANDEVULAR
                ActiveAppointments = appointments
                    .Where(a => a.Status == AppointmentStatus.Active && a.AppointmentDate >= DateTime.Today)
                    .Select(a => new AppointmentDto
                    {
                        Id = a.Id,
                        DateFormatted = a.AppointmentDate.ToString("dd MMMM yyyy"), // "20 Temmuz 2024" formatı
                        TimeFormatted = a.AppointmentTime.ToString(@"hh\:mm"),
                        DoctorName = $"Dr. {a.Doctor.FullName}",
                        DepartmentName = a.Doctor.Department?.Name ?? "Bölüm Belirtilmemiş",
                        CanCancel = true
                    }).ToList(),

                // GEÇMİŞ RANDEVULAR
                PastAppointments = appointments
                    .Where(a => a.Status != AppointmentStatus.Active || a.AppointmentDate < DateTime.Today)
                    .Select(a => new AppointmentDto
                    {
                        Id = a.Id,
                        DateFormatted = a.AppointmentDate.ToString("dd MMMM yyyy"),
                        TimeFormatted = a.AppointmentTime.ToString(@"hh\:mm"),
                        DoctorName = $"Dr. {a.Doctor.FullName}",
                        DepartmentName = a.Doctor.Department?.Name ?? "Bölüm Belirtilmemiş",


                        // Tasarımdaki Badge Renkleri ve Etiketleri
                        StatusLabel = a.Status switch
                        {
                            AppointmentStatus.Completed => "Tamamlandı",
                            AppointmentStatus.Cancelled => "İptal Edildi",
                            _ => "Kaçırıldı"
                        },
                        StatusClass = a.Status switch
                        {
                            AppointmentStatus.Completed => "badge bg-success",
                            AppointmentStatus.Cancelled => "badge bg-danger",
                            _ => "badge bg-warning text-dark"
                        }
                    }).ToList()
            };
        }
        // 2. MÜSAİT SAATLERİ HESAPLAMA 
        public async Task<List<string>> GetAvailableSlotsAsync(int doctorId, DateTime date)
        {
            var takenTimes = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date && a.Status == AppointmentStatus.Active)
                .Select(a => a.AppointmentTime) 
                .ToListAsync();

            List<string> availableSlots = new List<string>();
            TimeSpan startHour = new TimeSpan(9, 0, 0);
            TimeSpan endHour = new TimeSpan(17, 0, 0);
            TimeSpan currentTime = DateTime.Now.TimeOfDay;
            bool isToday = date.Date == DateTime.Today;

            if (isToday && currentTime >= endHour)
            {
                return availableSlots; 
            }

            while (startHour < endHour)
            {
                if (isToday && startHour <= currentTime)
                {
                    startHour = startHour.Add(TimeSpan.FromMinutes(30));
                    continue;
                }

                if (!takenTimes.Contains(startHour))
                {
                    availableSlots.Add(startHour.ToString(@"hh\:mm"));
                }
                startHour = startHour.Add(TimeSpan.FromMinutes(30));
            }
            return availableSlots;
        }

        // 3. YENİ RANDEVU OLUŞTURMA
        public async Task<(bool IsSuccess, string Message)> CreateAppointmentAsync(int doctorId, int patientId, DateTime date, TimeSpan time)
        {
            // Mükerrer Kontrolü
            bool isTaken = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == doctorId &&
                a.AppointmentDate.Date == date.Date &&
                a.AppointmentTime == time && 
                a.Status == AppointmentStatus.Active);

            if (isTaken) return (false, "Seçilen saat maalesef dolu.");

            var appointment = new Appointment
            {
                DoctorId = doctorId,
                PatientId = patientId,
                AppointmentDate = date,
                AppointmentTime = time, 
                Status = AppointmentStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return (true, "Randevu oluşturuldu.");
        }
        // Tüm bölümleri getir
        public async Task<List<string>> GetAllDepartmentNamesAsync()
        {
            return await _context.Departments
                .Select(d => d.Name) 
                .OrderBy(n => n)
                .ToListAsync();
        }

        // Bölüme göre doktorları filtrele
        public async Task<List<Doctor>> GetDoctorsByDepartmentNameAsync(string departmentName)
        {
            return await _context.Doctors
                .Include(d => d.Department) // İlişkili tablo join edilir. 
                .Where(d => d.Department.Name == departmentName) // Nesne içindeki Name ile kıyasla
                .OrderBy(d => d.FullName)
                .ToListAsync();
        }

        // 4. İPTAL ETME
        public async Task<bool> CancelAppointmentAsync(int appointmentId, int patientId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null || appointment.PatientId != patientId) return false;

            appointment.Status = AppointmentStatus.Cancelled; // Soft Delete / Durum güncelleme
            await _context.SaveChangesAsync();
            return true;
        }

        // 5. TEK RANDEVU GETİR (Edit İçin)
        public async Task<Appointment> GetAppointmentByIdAsync(int id)
        {
            return await _context.Appointments.FindAsync(id);
        }

        // 6. GÜNCELLEME
        public async Task<(bool IsSuccess, string Message)> UpdateAppointmentAsync(int id, Appointment appointment, int patientId)
        {
            if (id != appointment.Id) return (false, "ID uyuşmazlığı.");

            var existingApp = await _context.Appointments.FindAsync(id);
            if (existingApp == null || existingApp.PatientId != patientId) return (false, "Randevu bulunamadı.");

            // Doluluk Kontrolü (Kendisi hariç)
            bool isTaken = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == appointment.DoctorId &&
                a.AppointmentDate == appointment.AppointmentDate &&
                a.AppointmentTime == appointment.AppointmentTime &&
                a.Id != id &&
                a.Status == AppointmentStatus.Active);

            if (isTaken) return (false, "Bu saat dolu.");

            existingApp.AppointmentDate = appointment.AppointmentDate;
            existingApp.AppointmentTime = appointment.AppointmentTime;
            existingApp.DoctorId = appointment.DoctorId;

            _context.Update(existingApp);
            await _context.SaveChangesAsync();

            return (true, "Güncellendi.");
        }
       
        public async Task<DoctorDashboardViewModel> GetDoctorScheduleAsync(int doctorId)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .AsNoTracking()
                .ToListAsync();

            var today = DateTime.Today;

            return new DoctorDashboardViewModel
            {
                DoctorName = doctor?.FullName ?? "Doktor",
                TodaysAppointments = appointments
                    .Where(a => a.AppointmentDate.Date == today)
                    .Select(a => MapToDto(a)).ToList(),
                PastAppointments = appointments
                    .Where(a => a.AppointmentDate.Date < today)
                    .Select(a => MapToDto(a)).ToList()
            };
        }

        // Yardımcı Map metodu (DRY prensibi)
        private DoctorAppointmentDto MapToDto(Appointment a)
        {
            return new DoctorAppointmentDto
            {
                AppointmentId = a.Id,
                PatientId = a.PatientId,
                PatientName = a.Patient.FullName,
                Time = a.AppointmentTime.ToString(@"hh\:mm"),
                Date = a.AppointmentDate.ToString("dd MMMM yyyy"),
                StatusLabel = a.Status.ToString(), // Gereksinime göre Türkçeleştirilebilir
                StatusClass = a.Status switch
                {
                    AppointmentStatus.Active => "bg-blue-100 text-blue-700",
                    AppointmentStatus.Completed => "bg-green-100 text-green-700",
                    AppointmentStatus.Cancelled => "bg-red-100 text-red-700",
                    _ => "bg-gray-100 text-gray-700"
                }
            };
        }
        public async Task<List<Appointment>> GetAllAppointmentsAsync()
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .OrderByDescending(a => a.AppointmentDate)
                .AsNoTracking() // Performans için
                .ToListAsync();
        }
    }
}