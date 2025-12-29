using HastaneRandevuSistemi.Models;
using System.ComponentModel.DataAnnotations;

namespace HastaneRandevuSistemi.Models
{
    public class Doctor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Telefon numarasını 05xxxxxxxxx olarak giriniz.")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Telefon numarası sadece rakamlardan oluşmalıdır.")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public int DepartmentId { get; set; } // Foreign Key (Bölüm ID'si)

        public Department? Department { get; set; } // Navigation Property                                                   
        public List<Appointment> Appointments { get; set; } = new();
    }
}
