using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.Services;
using HastaneRandevuSistemi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HastaneRandevuSistemi.Controllers
{
    [Authorize] // Sadece giriş yapan kullanıcılar erişebilir
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        // ==========================================
        // 1. RANDEVU LİSTELEME (INDEX)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            int patientId = GetCurrentPatientId();
            if (patientId == 0) return RedirectToAction("Login", "Account");

            var model = await _appointmentService.GetPatientAppointmentsAsync(patientId);
            return View(model);
        }

        // ==========================================
        // 2. YENİ RANDEVU ALMA (CREATE - GET)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new AppointmentCreateViewModel
            {
                // Sayfa ilk açıldığında bölümlerin (Department) dolması gerekir
                Departments = await _appointmentService.GetAllDepartmentNamesAsync()
            };
            return View(model);
        }

        // ==========================================
        // 2.1 YENİ RANDEVU KAYDETME (CREATE - POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentCreateViewModel model)
        {
            int patientId = GetCurrentPatientId();
            if (patientId == 0) return RedirectToAction("Login", "Account");

            // === HAFTA SONU KONTROLÜ===
            if (model.SelectedDate.DayOfWeek == DayOfWeek.Saturday || model.SelectedDate.DayOfWeek == DayOfWeek.Sunday)
            {
                ModelState.AddModelError("", "Hafta sonları randevu alınamaz.");
            }

            if (ModelState.IsValid)
            {
                // Saat bilgisini TimeSpan'e dönüştür
                if (TimeSpan.TryParse(model.SelectedSlot, out TimeSpan time))
                {
                    var result = await _appointmentService.CreateAppointmentAsync(
                        model.SelectedDoctorId, patientId, model.SelectedDate, time);

                    if (result.IsSuccess)
                    {
                        TempData["SuccessMessage"] = result.Message;
                        return RedirectToAction(nameof(Index));
                    }

                    TempData["ErrorMessage"] = result.Message;
                }
                else
                {
                    ModelState.AddModelError("", "Lütfen geçerli bir saat seçiniz.");
                }
            }
            model.Departments = await _appointmentService.GetAllDepartmentNamesAsync();

            return View(model);
        }

        // ==========================================
        // 3. AJAX METOTLARI (DİNAMİK YAPI)
        // ==========================================

        // Bölüm seçilince doktorları getiren metot
        [HttpGet]
        public async Task<JsonResult> GetDoctors(string departmentName)
        {
            if (string.IsNullOrEmpty(departmentName)) return Json(new List<object>());

            var doctors = await _appointmentService.GetDoctorsByDepartmentNameAsync(departmentName);
            var result = doctors.Select(d => new { id = d.Id, fullName = d.FullName });

            return Json(result);
        }

        // Doktor ve Tarih seçilince müsait saatleri getiren metot
        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int doctorId, string dateStr)
        {
            if (!DateTime.TryParse(dateStr, out DateTime selectedDate))
                return BadRequest("Geçersiz tarih.");

            var slots = await _appointmentService.GetAvailableSlotsAsync(doctorId, selectedDate);
            return Json(slots);
        }

        // ==========================================
        // 4. RANDEVU İPTALİ (CANCEL)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            int patientId = GetCurrentPatientId();
            var result = await _appointmentService.CancelAppointmentAsync(id, patientId);

            if (result)
                TempData["SuccessMessage"] = "Randevunuz başarıyla iptal edildi.";
            else
                TempData["ErrorMessage"] = "İptal işlemi başarısız oldu.";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 5. YARDIMCI METOTLAR (HELPERS)
        // ==========================================
        private int GetCurrentPatientId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrEmpty(userIdString) ? 0 : int.Parse(userIdString);
        }
    }
}