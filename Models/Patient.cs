using System.ComponentModel.DataAnnotations;

namespace HastaneRandevuSistemi.Models
{
    public class Patient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(11)]
        public string IdentityNumber { get; set; } = string.Empty; // TC No

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        // === EN ÖNEMLÝ DEÐÝÞÝKLÝK ===
        [Required]
        public string PasswordHash { get; set; } = string.Empty; // Password -> PasswordHash

        [Phone]
        public string Phone { get; set; } = string.Empty;

        public List<Appointment> Appointments { get; set; } = new();
    }
}