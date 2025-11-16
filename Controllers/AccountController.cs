using HastaneRandevuSistemi.Services;
using HastaneRandevuSistemi.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HastaneRandevuSistemi.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        // 1. SEÇİM EKRANI: Hasta mı Doktor mu?
        [HttpGet]
        public IActionResult RegisterSelection()
        {
            return View();
        }

        // 2. HASTA KAYIT SAYFASI
        [HttpGet]
        public IActionResult RegisterPatient()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterPatient(PatientRegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _authService.RegisterPatientAsync(model);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Kayıt başarılı! Lütfen giriş yapınız.";
                return RedirectToAction("Login"); // Login sayfasına yönlendir
            }

            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        // 3. DOKTOR KAYIT SAYFASI
        [HttpGet]
        public IActionResult RegisterDoctor()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterDoctor(DoctorRegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _authService.RegisterDoctorAsync(model);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Doktor kaydı başarılı!";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        // Giriş Sayfası (Şimdilik boş)
        public IActionResult Login()
        {
            return View();
        }
    }
}