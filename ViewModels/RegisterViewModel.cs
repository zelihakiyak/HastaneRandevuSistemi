using System.ComponentModel.DataAnnotations;

namespace HastaneRandevuSistemi.ViewModels
{
    // HASTA KAYIT MODELİ
    public class PatientRegisterViewModel
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "TC Kimlik No zorunludur.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC No 11 haneli olmalıdır.")]
        public string IdentityNumber { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Telefon numarasını 05xxxxxxxxx olarak giriniz.")]
        [Display(Name = "Telefon Numarası")]
        public string Phone { get; set; }
    }

    // DOKTOR KAYIT MODELİ
    public class DoctorRegisterViewModel
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Branş seçimi zorunludur.")]
        public string Department { get; set; } // Dropdown ile seçtirilecek

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor.")]
        public string ConfirmPassword { get; set; }
        public string Phone { get; internal set; }
    }
}