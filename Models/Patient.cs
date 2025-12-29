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

        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon numarasý zorunludur.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Telefon numarasýný 05xxxxxxxxx olarak giriniz.")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Telefon numarasý sadece rakamlardan oluþmalýdýr.")]
        public string Phone { get; set; } = string.Empty;
        public List<Appointment> Appointments { get; set; } = new();
    }
}