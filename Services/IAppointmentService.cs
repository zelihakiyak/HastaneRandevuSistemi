using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.ViewModels; // ViewModel kullandığımızı varsayıyoruz

namespace HastaneRandevuSistemi.Services
{
    public interface IAppointmentService
    {
        // Listeleme (Index) için
        Task<AppointmentListViewModel> GetPatientAppointmentsAsync(int patientId);

        // Müsaitlik Kontrolü (GetAvailableSlots) için
        Task<List<string>> GetAvailableSlotsAsync(int doctorId, DateTime date);

        // Randevu Alma (Create) için
        Task<(bool IsSuccess, string Message)> CreateAppointmentAsync(int doctorId, int patientId, DateTime date, TimeSpan time);

        // Randevu İptali (Delete/Cancel) için
        Task<bool> CancelAppointmentAsync(int appointmentId, int patientId);

        // Düzenleme (Edit) Sayfası Getirirken
        Task<Appointment> GetAppointmentByIdAsync(int id);

        // Düzenleme Kaydetme (Edit Post)
        Task<(bool IsSuccess, string Message)> UpdateAppointmentAsync(int id, Appointment appointment, int patientId);
        // Tüm bölümleri getirmek için (İlk dropdown)
        Task<List<string>> GetAllDepartmentNamesAsync();

        // Seçilen bölüme ait doktorları getirmek için (İkinci dropdown)
        Task<List<Doctor>> GetDoctorsByDepartmentNameAsync(string departmentName);

        Task<DoctorDashboardViewModel> GetDoctorScheduleAsync(int doctorId);

        Task<List<Appointment>> GetAllAppointmentsAsync();
        // Servisinize bu mantıkta bir kontrol ekleyin veya Controller'da kullanın
        public bool IsDateAvailable(DateTime selectedDate)
        {
            var today = DateTime.Today;
            var now = DateTime.Now;

            // 1. Geçmiş tarih kontrolü
            if (selectedDate < today) return false;

            // 2. Hafta sonu kontrolü
            if (selectedDate.DayOfWeek == DayOfWeek.Saturday || selectedDate.DayOfWeek == DayOfWeek.Sunday) return false;

            // 3. SAAT 17:00 KONTROLÜ
            // Eğer bugün seçiliyse ve saat 17:00'yi geçtiyse bu tarih aktif olmamalı
            if (selectedDate == today && now.Hour >= 17)
            {
                return false;
            }

            return true;
        }
    }
}