using System.ComponentModel.DataAnnotations;

namespace HastaneRandevuSistemi.Models
{
    public class Doctor
    {
        [Key]
        public int Id { get; set; } // DoctorID -> Id olarak değiştirildi (EF Core standardı)

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        // === EN ÖNEMLİ DEĞİŞİKLİK ===
        // Düz metin şifreyi kaldırıp, şifrelenmiş (hash) halini tutuyoruz.
        [Required]
        public string PasswordHash { get; set; } = string.Empty; // Password -> PasswordHash

        [Required]
        public string Department { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        public List<Appointment> Appointments { get; set; } = new();
    }
}