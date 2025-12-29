using System.Diagnostics;
using HastaneRandevuSistemi.Models;
using Microsoft.AspNetCore.Mvc;

namespace HastaneRandevuSistemi.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Kullanýcý giriþ yapmamýþsa genel tanýtým sayfasýný göster (veya Login'e at)
            if (!User.Identity.IsAuthenticated)
            {
                return View(); // Veya return RedirectToAction("Login", "Account");
            }

            // Kullanýcý giriþ yapmýþsa rolüne göre yönlendir
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }
            else if (User.IsInRole("Doctor"))
            {
                return RedirectToAction("Index", "Doctor");
            }
            else if (User.IsInRole("Patient"))
            {
                // Hastalarý genellikle randevu listesine veya randevu al sayfasýna atarýz
                return RedirectToAction("Index", "Appointment");
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
