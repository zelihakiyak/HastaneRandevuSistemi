using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.ViewModels;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net; 

namespace HastaneRandevuSistemi.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // METOT 1: HASTA KAYIT (BCrypt ile)
        // ============================================================
        public async Task<(bool IsSuccess, string Message)> RegisterPatientAsync(PatientRegisterViewModel model)
        {
            if (await _context.Patients.AnyAsync(x => x.Email == model.Email))
                return (false, "Bu e-posta adresi zaten kayıtlı.");

            if (await _context.Patients.AnyAsync(x => x.IdentityNumber == model.IdentityNumber))
                return (false, "Bu TC Kimlik numarası zaten kayıtlı.");

            var patient = new Patient
            {
                FullName = model.FullName,
                IdentityNumber = model.IdentityNumber,
                Email = model.Email,
                // Şifreyi BCrypt ile güvenli bir şekilde hashliyoruz
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return (true, "Kayıt başarılı.");
        }

        // ============================================================
        // METOT 2: DOKTOR KAYIT (Admin tarafından kullanılacak)
        // ============================================================       
        public async Task<(bool IsSuccess, string Message)> RegisterDoctorAsync(DoctorRegisterViewModel model)
        {
            if (await _context.Doctors.AnyAsync(x => x.Email == model.Email))
                return (false, "Bu e-posta adresi zaten kayıtlı.");

            var doctor = new Doctor
            {
                FullName = model.FullName,
                Email = model.Email,
                // Admin'in belirlediği şifreyi hashleyerek kaydediyoruz
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Phone = model.Phone,
                DepartmentId = int.Parse(model.Department)

            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return (true, "Doktor sisteme başarıyla tanımlandı.");
        }

        // ============================================================
        // METOT 3: HASTA GİRİŞ DOĞRULAMA (BCrypt.Verify ile)
        // ============================================================
        public async Task<Patient?> ValidatePatientCredentialsAsync(LoginViewModel model)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Email == model.Email);
            
            // Kullanıcı varsa ve şifresi BCrypt ile doğrulanıyorsa (Verify)
            if (patient != null && BCrypt.Net.BCrypt.Verify(model.Password, patient.PasswordHash))
            {
                return patient;
            }
            
            return null;
        }

        // ============================================================
        // METOT 4: DOKTOR GİRİŞ DOĞRULAMA (BCrypt.Verify ile)
        // ============================================================
        public async Task<Doctor?> ValidateDoctorCredentialsAsync(LoginViewModel model)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Email == model.Email);
            
            // Kullanıcı varsa ve şifresi BCrypt ile doğrulanıyorsa (Verify)
            if (doctor != null && BCrypt.Net.BCrypt.Verify(model.Password, doctor.PasswordHash))
            {
                return doctor;
            }
            
            return null;
        }
    }
}