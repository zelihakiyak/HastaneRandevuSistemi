namespace HastaneRandevuSistemi.ViewModels
{
    public class MyPatientViewModel
    {
        // Hastanın tam adı
        public string FullName { get; set; } = string.Empty;

        // T.C. Kimlik Numarası (11 hane kuralına uygun gelecek)
        public string IdentityNumber { get; set; } = string.Empty;

        // Hastanın telefon numarası (11 hane kuralına uygun gelecek)
        public string Phone { get; set; } = string.Empty;

        // Bu hastanın doktora geldiği son tarih
        public DateTime LastAppointmentDate { get; set; }
    }
}