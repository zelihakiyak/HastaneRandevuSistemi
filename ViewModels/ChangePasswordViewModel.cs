using System.ComponentModel.DataAnnotations;

namespace HastaneRandevuSistemi.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mevcut şifrenizi giriniz.")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifrenizi giriniz.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Şifreler uyuşmuyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}