using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.Services;
using HastaneRandevuSistemi.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HastaneRandevuSistemi.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _context;

        public AccountController(IAuthService authService, ApplicationDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        // ============================================================
        // === HASTA KAYIT (Sadece Hastalar Kendi Kayıt Olabilir) ===
        // ============================================================

        [HttpGet]
        public IActionResult Register() 
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(PatientRegisterViewModel model)
        {
            if (!ModelState.IsValid) return View("Register", model);

            var result = await _authService.RegisterPatientAsync(model);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Kayıt başarılı! Lütfen giriş yapınız.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", result.Message);
            return View("Register", model);
        }

        // ============================================================
        // === MERKEZİ GİRİŞ SİSTEMİ (Role Based Login) ===
        // ============================================================

        [HttpGet]
        public IActionResult Login()
        {
            // Kullanıcı zaten giriş yapmışsa, rolüne göre "kendi dünyasına" gönder
            if (User.Identity.IsAuthenticated)
            {
                return RedirectUserByRole();
            }

            return View(new LoginViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // 1. ADIM: ADMIN KONTROLÜ 
            // Veritabanındaki Admins tablosu sorgulanıyor.
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == model.Email);
            if (admin != null)
            {
                // Veritabanındaki Hash'li şifre ile girilen şifreyi doğrula.
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, admin.PasswordHash);

                if (isPasswordValid)
                {
                    await CreateAuthCookie(admin.Id, admin.FullName, admin.Email, "Admin", model.RememberMe);
                    return RedirectToAction("Index", "Admin");
                }
            }

            // 2. ADIM: DOKTOR KONTROLÜ
            // Doktor veritabanı üzerinden doğrulanıyor.
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Email == model.Email);
            if (doctor != null)
            {
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, doctor.PasswordHash);

                if (isPasswordValid)
                {
                    await CreateAuthCookie(doctor.Id, doctor.FullName, doctor.Email, "Doctor", model.RememberMe);
                    return RedirectToAction("Index", "Doctor");
                }
            }

            // 3. ADIM: HASTA KONTROLÜ
            // Hasta veritabanı üzerinden doğrulanıyor.
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Email == model.Email);
            if (patient != null)
            {
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, patient.PasswordHash);

                if (isPasswordValid)
                {
                    await CreateAuthCookie(patient.Id, patient.FullName, patient.Email, "Patient", model.RememberMe);
                    return RedirectToAction("Create", "Appointment");
                }
            }

            // Hata Durumu
            ModelState.AddModelError("", "E-posta adresi veya şifre hatalı.");
            return View(model);
        }

        // ============================================================
        // === ÇIKIŞ VE YARDIMCI METOTLAR ===
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private async Task CreateAuthCookie(int id, string name, string email, string role, bool isPersistent)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = isPersistent };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        private IActionResult RedirectUserByRole()
        {
            if (User.IsInRole("Admin")) return RedirectToAction("Index", "Admin");
            if (User.IsInRole("Doctor")) return RedirectToAction("Index", "Doctor");
            if (User.IsInRole("Patient")) return RedirectToAction("Create", "Appointment");
            return RedirectToAction("Index", "Home");
        }

    }
}