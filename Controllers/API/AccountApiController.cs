using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.Services;
using HastaneRandevuSistemi.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HastaneRandevuSistemi.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountApiController(IAuthService _authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] PatientRegisterViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authService.RegisterPatientAsync(model);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            // Not: API tarafında genellikle JWT Token dönülür. 
            // Şimdilik MVC mantığındaki doğrulamayı simüle ediyoruz.
            return Ok(new { message = "Giriş başarılı (API test)" });
        }
    }
}