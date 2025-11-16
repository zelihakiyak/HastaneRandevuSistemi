using HastaneRandevuSistemi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HastaneRandevuSistemi.Models;

namespace HastaneRandevuSistemi.Controllers
{
    public class DoctorController(ApplicationDbContext _context) : Controller
    {
        // -----------------------------------------------------
        // REGISTER (GET)
        // -----------------------------------------------------
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // -----------------------------------------------------
        // REGISTER (POST)
        // -----------------------------------------------------
        [HttpPost]
        public IActionResult Register(Doctor doctor)
        {
            if (!ModelState.IsValid)
                return View(doctor);

            // E-posta zaten var mı kontrol et
            var existingDoctor = _context.Doctors
                .FirstOrDefault(d => d.Email == doctor.Email);

            if (existingDoctor != null)
            {
                ViewBag.Error = "Bu e-posta zaten kayıtlı!";
                return View(doctor);
            }

            // Kaydet
            _context.Doctors.Add(doctor);
            _context.SaveChanges();

            TempData["Success"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
            return RedirectToAction("Login");
        }

        // -----------------------------------------------------
        // LOGIN (GET)
        // -----------------------------------------------------
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // -----------------------------------------------------
        // LOGIN (POST)
        // -----------------------------------------------------
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var doctor = _context.Doctors
                .FirstOrDefault(d => d.Email == email && d.PasswordHash == password);

            if (doctor == null)
            {
                ViewBag.Error = "Hatalı mail veya şifre!";
                return View();
            }

            // Login başarılı → dashboard'a gönder
            HttpContext.Session.SetString("DoctorId", doctor.Id.ToString());
            return RedirectToAction("Dashboard");
        }

        // -----------------------------------------------------
        // DASHBOARD
        // -----------------------------------------------------
        public IActionResult Dashboard()
        {
            string? id = HttpContext.Session.GetString("DoctorId");

            if (id == null)
                return RedirectToAction("Login");

            var doctor = _context.Doctors.Find(int.Parse(id));

            return View(doctor);
        }

        // -----------------------------------------------------
        // LOGOUT
        // -----------------------------------------------------
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}