using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientApiController(ApplicationDbContext _context) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _context.Patients.ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            return patient == null ? NotFound() : Ok(patient);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();
            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}