using HastaneRandevuSistemi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace HastaneRandevuSistemi.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorController(ApplicationDbContext _context) : Controller
    {
        private int GetCurrentDoctorId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // 1. GELECEK RANDEVULAR (Dashboard / Index)
        public async Task<IActionResult> Index()
        {
            var doctorId = GetCurrentDoctorId();
            var today = DateTime.Today;

            // Bugün ve gelecekteki randevular
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate >= today && a.Status == AppointmentStatus.Active)
                .OrderBy(a => a.AppointmentDate).ThenBy(a => a.AppointmentTime)
                .ToListAsync();

            return View(appointments);

        }
        // 2. GEÇMİŞ RANDEVULAR
        public async Task<IActionResult> PastAppointments()
        {
            var doctorId = GetCurrentDoctorId();
            var now = DateTime.Now;

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId && (a.AppointmentDate < now.Date || a.Status != AppointmentStatus.Active))
                .OrderByDescending(a => a.AppointmentDate).ThenByDescending(a => a.AppointmentTime)
                .ToListAsync();

            // View içerisinde bu listeyi dönerken:
            // 1. Durum 'Cancelled' ise -> İptal
            // 2. Durum 'Missed' ise -> Gelmedi
            // 3. Durum hala 'Active' ama zamanı geçmişse -> Tamamlandı (Doktorun yükü sıfır!)
            return View(appointments);
        }
        // 3. HASTALARIM
        public async Task<IActionResult> MyPatients()
        {
            var doctorId = GetCurrentDoctorId();

            var patients = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId)
                .Select(a => new MyPatientViewModel
                {
                    FullName = a.Patient.FullName,
                    IdentityNumber = a.Patient.IdentityNumber,
                    Phone = a.Patient.Phone,
                    LastAppointmentDate = a.AppointmentDate
                })
                .ToListAsync();

            var distinctPatients = patients
                .GroupBy(p => p.IdentityNumber)
                .Select(g => g.OrderByDescending(x => x.LastAppointmentDate).First())
                .ToList();

            return View(distinctPatients);
        }
        // -----------------------------------------------------
        // 4. HASTA GELMEDİ OLARAK İŞARETLE
        // -----------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsMissed(int id)
        {
            var doctorId = GetCurrentDoctorId();

            // Güvenlik: Sadece giriş yapan doktorun kendi randevusunu güncellemesini sağlıyoruz
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctorId);

            if (appointment == null)
            {
                return NotFound();
            }

            // Durumu 'Missed' (Kaçırıldı) olarak güncelle
            appointment.Status = AppointmentStatus.Missed;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Hasta gelmedi olarak işaretlendi.";
            return RedirectToAction("Index");
        }
    }
}