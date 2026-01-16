using System.ComponentModel.DataAnnotations;

namespace HastaneRandevuSistemi.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Bölüm adı zorunludur.")]
        public string Name { get; set; } = string.Empty;

        public List<Doctor> Doctors { get; set; } = new();
    }
}