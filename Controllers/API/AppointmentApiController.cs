using Microsoft.AspNetCore.Mvc;
using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.Services;
using HastaneRandevuSistemi.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace HastaneRandevuSistemi.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class AppointmentApiController(IAppointmentService _appointmentService) : ControllerBase
    {
        private int GetCurrentPatientId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrEmpty(userIdString) ? 0 : int.Parse(userIdString);
        }

        // 1. HASTA ID'SİNE GÖRE RANDEVULARI LİSTELE (GET)
        [HttpGet("my-appointments/{patientId}")]
        public async Task<IActionResult> GetMyAppointments(int patientId) // Parametre eklendi
        {
            // Güvenlik Kontrolü (İsteğe bağlı): 
            // Eğer sadece giriş yapan kullanıcı kendi randevusunu görsün istiyorsanız 
            // burada bir yetki kontrolü yapabilirsiniz.

            if (patientId <= 0)
            {
                return BadRequest("Geçerli bir hasta ID'si belirtilmelidir.");
            }

            // Servis katmanını kullanarak veritabanından randevuları getiriyoruz
            var appointments = await _appointmentService.GetPatientAppointmentsAsync(patientId);

            if (appointments == null)
            {
                return NotFound($"{patientId} ID'li hastaya ait randevu bulunamadı.");
            }

            return Ok(appointments); // JSON formatında randevu listesini döner
        }

        // 2. TÜM BÖLÜMLERİ GETİR (GET)
        [HttpGet("departments")]
        [AllowAnonymous] 
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _appointmentService.GetAllDepartmentNamesAsync();
            return Ok(departments);
        }

        // 3. SEÇİLİ BÖLÜME GÖRE DOKTORLARI GETİR (GET)
        [HttpGet("doctors/{departmentName}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDoctors(string departmentName)
        {
            var doctors = await _appointmentService.GetDoctorsByDepartmentNameAsync(departmentName);
            var result = doctors.Select(d => new { d.Id, d.FullName });
            return Ok(result);
        }

        // 4. MÜSAİT SAATLERİ (SLOTS) SORGULA (GET)
        [HttpGet("available-slots")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSlots(int doctorId, DateTime date)
        {
            // Hafta sonu ise boş liste dön
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                return Ok(new List<string>());
            }

            var slots = await _appointmentService.GetAvailableSlotsAsync(doctorId, date);
            return Ok(slots);
        }

        // 5. YENİ RANDEVU OLUŞTUR (POST)
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] AppointmentCreateViewModel model)
        {
            int patientId = GetCurrentPatientId();
            if (patientId == 0) return Unauthorized();

            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (TimeSpan.TryParse(model.SelectedSlot, out TimeSpan time))
            {
                var result = await _appointmentService.CreateAppointmentAsync(
                    model.SelectedDoctorId, patientId, model.SelectedDate, time);

                return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
            }

            return BadRequest("Geçersiz saat formatı.");
        }

        // 6. RANDEVU İPTAL ET (POST/DELETE)
        [HttpPost("cancel/{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            int patientId = GetCurrentPatientId();
            var success = await _appointmentService.CancelAppointmentAsync(id, patientId);

            return success ? Ok(new { message = "İptal başarılı." }) : BadRequest("İptal işlemi başarısız.");
        }
    }
}