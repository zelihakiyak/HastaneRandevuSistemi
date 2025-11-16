using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HastaneRandevuSistemi.Services
{
    // Bu sınıf, IAuthService arayüzündeki (sözleşmedeki) tüm metotları
    // eksiksiz olarak uygulamak zorundadır.
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // METOT 1: HASTA KAYIT
        // ============================================================
        public async Task<(bool IsSuccess, string Message)> RegisterPatientAsync(PatientRegisterViewModel model)
        {
            // E-posta veya TC zaten var mı?
            if (await _context.Patients.AnyAsync(x => x.Email == model.Email))
                return (false, "Bu e-posta adresi zaten kayıtlı.");

            if (await _context.Patients.AnyAsync(x => x.IdentityNumber == model.IdentityNumber))
                return (false, "Bu TC Kimlik numarası zaten kayıtlı.");

            // Yeni Patient nesnesi oluştur
            var patient = new Patient
            {
                FullName = model.FullName,
                IdentityNumber = model.IdentityNumber,
                Email = model.Email,
                // Güvenlik: Şifreyi Hash'leyerek PasswordHash alanına kaydet
                PasswordHash = HashPassword(model.Password)
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return (true, "Kayıt başarılı.");
        }

        // ============================================================
        // METOT 2: DOKTOR KAYIT
        // ============================================================
        public async Task<(bool IsSuccess, string Message)> RegisterDoctorAsync(DoctorRegisterViewModel model)
        {
            if (await _context.Doctors.AnyAsync(x => x.Email == model.Email))
                return (false, "Bu e-posta adresi zaten kayıtlı.");

            var doctor = new Doctor
            {
                // DÜZELTME: Modelinizdeki alan adı 'FullName'
                FullName = model.FullName,
                Department = model.Department,
                Email = model.Email,
                // Güvenlik: Şifreyi Hash'leyerek PasswordHash alanına kaydet
                PasswordHash = HashPassword(model.Password)
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return (true, "Doktor kaydı başarılı.");
        }

        // ============================================================
        // METOT 3: HASTA GİRİŞ DOĞRULAMA
        // ============================================================
        public async Task<Patient> ValidatePatientCredentialsAsync(LoginViewModel model)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Email == model.Email);
            if (patient == null) return null; // Kullanıcı bulunamadı

            // Girilen şifreyi de hash'le
            var passwordHash = HashPassword(model.Password);

            // Veritabanındaki hash ile girilen şifrenin hash'i aynı mı?
            if (patient.PasswordHash == passwordHash)
            {
                return patient; // Başarılı
            }
            return null; // Şifre yanlış
        }

        // ============================================================
        // METOT 4: DOKTOR GİRİŞ DOĞRULAMA
        // ============================================================
        public async Task<Doctor> ValidateDoctorCredentialsAsync(LoginViewModel model)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Email == model.Email);
            if (doctor == null) return null; // Kullanıcı bulunamadı

            var passwordHash = HashPassword(model.Password);

            // Veritabanındaki hash ile girilen şifrenin hash'i aynı mı?
            if (doctor.PasswordHash == passwordHash)
            {
                return doctor; // Başarılı
            }
            return null; // Şifre yanlış
        }

        // ============================================================
        // YARDIMCI METOT: ŞİFRELEME (Hashing)
        // ============================================================
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}