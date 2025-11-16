using HastaneRandevuSistemi.Data; // Sadece SelectList için gerekebilir, gerekirse Service'den çekilir.
using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HastaneRandevuSistemi.Controllers
{
    // [Authorize] 
    public class AppointmentController : Controller
    {
        // ARTIK _context YOK! Sadece Service var.
        private readonly IAppointmentService _appointmentService;
        private readonly ApplicationDbContext _context; // Sadece Dropdown doldurmak için geçici (Clean Code'da bu da service'e taşınmalı)

        public AppointmentController(IAppointmentService appointmentService, ApplicationDbContext context)
        {
            _appointmentService = appointmentService;
            _context = context; // SelectList için tutuyoruz şimdilik
        }

        // 1. INDEX (LİSTELEME)
        public async Task<IActionResult> Index()
        {
            int currentPatientId = GetCurrentPatientId();
            var model = await _appointmentService.GetPatientAppointmentsAsync(currentPatientId);
            return View(model);
        }

        // 2. CREATE (GET)
        [HttpGet]
        public IActionResult Create()
        {
            // İdealde: var doctors = await _doctorService.GetAllDoctors();
            ViewBag.Doctors = new SelectList(_context.Doctors, "Id", "Name");
            return View();
        }

        // 2.1 CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int doctorId, DateTime date, string timeStr)
        {
            // Validasyonlar
            if (!TimeSpan.TryParse(timeStr, out TimeSpan time))
            {
                ModelState.AddModelError("", "Geçersiz saat.");
                ViewBag.Doctors = new SelectList(_context.Doctors, "Id", "Name");
                return View();
            }

            int currentPatientId = GetCurrentPatientId();

            // Servisi Çağır
            var result = await _appointmentService.CreateAppointmentAsync(doctorId, currentPatientId, date, time);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Message);
                ViewBag.Doctors = new SelectList(_context.Doctors, "Id", "Name");
                return View();
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // 3. AJAX (MÜSAİT SAATLER)
        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int doctorId, string dateStr)
        {
            if (!DateTime.TryParse(dateStr, out DateTime selectedDate))
                return BadRequest("Geçersiz tarih.");

            var slots = await _appointmentService.GetAvailableSlotsAsync(doctorId, selectedDate);
            return Json(slots);
        }

        // 4. EDIT (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null) return NotFound();

            if (appointment.PatientId != GetCurrentPatientId()) return Unauthorized();

            ViewBag.Doctors = new SelectList(_context.Doctors, "Id", "Name", appointment.DoctorId);
            return View(appointment);
        }

        // 5. EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Appointment appointment)
        {
            int currentPatientId = GetCurrentPatientId();
            var result = await _appointmentService.UpdateAppointmentAsync(id, appointment, currentPatientId);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Message);
                ViewBag.Doctors = new SelectList(_context.Doctors, "Id", "Name", appointment.DoctorId);
                return View(appointment);
            }

            return RedirectToAction(nameof(Index));
        }

        // 6. CANCEL / DELETE (İPTAL)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            int currentPatientId = GetCurrentPatientId();
            bool success = await _appointmentService.CancelAppointmentAsync(id, currentPatientId);

            if (success) TempData["SuccessMessage"] = "Randevu iptal edildi.";
            else TempData["ErrorMessage"] = "İptal edilemedi.";

            return RedirectToAction(nameof(Index));
        }

        private int GetCurrentPatientId()
        {
            // Test için sabit
            return 1;
        }
    }
}