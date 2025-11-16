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
                .Where(a => a.PatientId == patientId) // Hepsini çekiyoruz, status kontrolünü aşağıda yapacağız
                .OrderByDescending(a => a.AppointmentDate)
                .AsNoTracking()
                .ToListAsync();

            // Buradaki mapleme işlemini dilerseniz AutoMapper ile de yapabilirsiniz
            return new AppointmentListViewModel
            {
                ActiveAppointments = appointments
                    .Where(a => a.Status == AppointmentStatus.Active && a.AppointmentDate >= DateTime.Now)
                    .Select(a => new AppointmentDto
                    {
                        Id = a.Id,
                        DateFormatted = a.AppointmentDate.ToString("dd.MM.yyyy"),
                        TimeFormatted = a.AppointmentTime.ToString(@"hh\:mm"), // TimeSpan ise
                        DoctorName = a.Doctor.FullName,
                        CanCancel = true
                    }).ToList(),

                PastAppointments = appointments
                    .Where(a => a.Status != AppointmentStatus.Active || a.AppointmentDate < DateTime.Now)
                    .Select(a => new AppointmentDto
                    {
                        Id = a.Id,
                        DateFormatted = a.AppointmentDate.ToString("dd.MM.yyyy"),
                        TimeFormatted = a.AppointmentTime.ToString(@"hh\:mm"),
                        DoctorName = a.Doctor.FullName,
                        StatusLabel = a.Status.ToString()
                    }).ToList()
            };
        }

        // 2. MÜSAİT SAATLERİ HESAPLAMA (Logic Controller'dan buraya taşındı)
        public async Task<List<string>> GetAvailableSlotsAsync(int doctorId, DateTime date)
        {
            var takenTimes = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date && a.Status == AppointmentStatus.Active)
                .Select(a => a.AppointmentTime) // TimeSpan varsayıyoruz
                .ToListAsync();

            List<string> availableSlots = new List<string>();
            TimeSpan startHour = new TimeSpan(9, 0, 0);
            TimeSpan endHour = new TimeSpan(17, 0, 0);
            TimeSpan currentTime = DateTime.Now.TimeOfDay;
            bool isToday = date.Date == DateTime.Today;

            while (startHour < endHour)
            {
                if (isToday && startHour <= currentTime)
                {
                    startHour = startHour.Add(TimeSpan.FromHours(1));
                    continue;
                }

                if (!takenTimes.Contains(startHour))
                {
                    availableSlots.Add(startHour.ToString(@"hh\:mm"));
                }
                startHour = startHour.Add(TimeSpan.FromHours(1));
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
                a.AppointmentTime == time && // TimeSpan ise
                a.Status == AppointmentStatus.Active);

            if (isTaken) return (false, "Seçilen saat maalesef dolu.");

            var appointment = new Appointment
            {
                DoctorId = doctorId,
                PatientId = patientId,
                AppointmentDate = date,
                AppointmentTime = time, // Modelde Time ayrı ise
                Status = AppointmentStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return (true, "Randevu oluşturuldu.");
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
    }
}