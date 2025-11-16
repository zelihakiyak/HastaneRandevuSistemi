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
    }
}