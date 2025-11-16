using Microsoft.AspNetCore.Mvc;

namespace HastaneRandevuSistemi.Controllers
{
    public class AdminController : Controller
    {
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string Email, string Password)
        {
            if (Email == "admin@mail.com" && Password == "admin123")
                return RedirectToAction("Dashboard");

            ViewBag.Error = "Invalid credentials";
            return View();
        }

        public IActionResult Dashboard() => View();
    }
}
