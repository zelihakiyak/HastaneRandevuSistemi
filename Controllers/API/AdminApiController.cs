using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminApiController(ApplicationDbContext _context) : ControllerBase
    {
        [HttpGet("doctors")]
        public async Task<IActionResult> GetDoctors() => Ok(await _context.Doctors.Include(d => d.Department).ToListAsync());

        [HttpPost("doctors")]
        public async Task<IActionResult> CreateDoctor([FromBody] Doctor doctor)
        {
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetDoctors), new { id = doctor.Id }, doctor);
        }

        [HttpDelete("doctors/{id}")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();
            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments() => Ok(await _context.Departments.ToListAsync());

        [HttpDelete("departments/{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            // 1. Bölümü ve içindeki doktorları kontrol et.
            var department = await _context.Departments
                .Include(d => d.Doctors)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                return NotFound(new { message = "Bölüm bulunamadı." });
            }

            // 2. Güvenlik Kontrolü: Eğer bölümde doktor varsa silme işlemine izin verme 
            if (department.Doctors.Any())
            {
                return BadRequest(new
                {
                    message = $"Bu bölümde ({department.Name}) kayıtlı doktorlar bulunduğu için silme işlemi yapılamaz."
                });
            }

            // 3. Silme işlemini gerçekleştirilir.
            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Bölüm başarıyla silindi." });
        }

    }
}