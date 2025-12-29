using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorApiController(ApplicationDbContext _context) : ControllerBase
    {
        [HttpGet("upcoming/{doctorId}")]
        public async Task<IActionResult> GetUpcoming(int doctorId)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate >= DateTime.Today)
                .ToListAsync();
            return Ok(appointments);
        }

        [HttpPost("mark-as-missed/{id}")]
        public async Task<IActionResult> MarkAsMissed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();
            appointment.Status = AppointmentStatus.Missed;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Hasta gelmedi olarak işaretlendi." });
        }

        [HttpGet("past/{doctorId}")]
        public async Task<IActionResult> GetPastAppointments(int doctorId)
        {
            var now = DateTime.Now;

            // MVC tarafındaki mantığın aynısını uyguluyoruz:
            // Tarihi bugünden eski olanlar VEYA durumu aktif olmayanlar geçmiş randevudur.
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId &&
                           (a.AppointmentDate < now.Date || a.Status != AppointmentStatus.Active))
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .Select(a => new {
                    a.Id,
                    a.AppointmentDate,
                    a.AppointmentTime,
                    PatientName = a.Patient.FullName,
                    // Akıllı durum bilgisini API'den doğrudan string olarak dönmek kolaylık sağlar
                    Status = (a.Status == AppointmentStatus.Completed || (a.Status == AppointmentStatus.Active && a.AppointmentDate < now.Date))
                             ? "Tamamlandı"
                             : (a.Status == AppointmentStatus.Cancelled ? "İptal Edildi" : "Gelmedi")
                })
                .ToListAsync();

            return Ok(appointments);
        }
    }
}