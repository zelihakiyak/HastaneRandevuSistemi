using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController(ApplicationDbContext _context, IAppointmentService _appointmentService) : Controller
    {
        // GET: Admin/AllAppointments
        public async Task<IActionResult> AllAppointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Department)
                .OrderByDescending(a => a.AppointmentDate) 
                .ThenByDescending(a => a.AppointmentTime)
                .ToListAsync();

            return View(appointments);
        }

        // -----------------------------------------------------
        // DOKTOR LİSTESİ
        // -----------------------------------------------------
        public async Task<IActionResult> Index()
        {
            // .Include(d => d.Department) ekleyerek doktorla birlikte 
            // bağlı olduğu bölüm bilgilerini de tek seferde çekiyoruz.
            var doctors = await _context.Doctors
                .Include(d => d.Department)
                .ToListAsync();

            return View(doctors);
        }

        // -----------------------------------------------------
        // YENİ DOKTOR EKLE (GET)
        // -----------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> CreateDoctor()
        {
            // Veritabanındaki tüm bölümleri listeliyoruz
            var departments = await _context.Departments.ToListAsync();

            // Bölüm listesini sayfada kullanabilmek için ViewBag'e atıyoruz
            ViewBag.Departments = departments;

            return View();
        }

        // -----------------------------------------------------
        // YENİ DOKTOR EKLE (POST)
        // -----------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDoctor(Doctor doctor, string password)
        {
            ModelState.Remove("PasswordHash");
            //ModelState.Remove("Department");
            // HATA AYIKLAMA: Eğer model geçersizse, nedenini bulalım
            if (!ModelState.IsValid)
            {
                // View tarafında @Html.ValidationSummary(false) varsa hatalar listelenir
                ViewBag.Departments = await _context.Departments.ToListAsync();
                return View(doctor);
            }

            try
            {
                if (!string.IsNullOrEmpty(password))
                {
                    doctor.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                }

                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Doktor başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Veritabanı hatası olursa buraya düşer (Örn: Email çakışması)
                ModelState.AddModelError("", "Veritabanına kaydedilirken hata oluştu: " + ex.Message);
                ViewBag.Departments = await _context.Departments.ToListAsync();
                return View(doctor);
            }
        }
        // -----------------------------------------------------
        // DOKTOR SİL
        // -----------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Doktor sistemden silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
        // GET: Admin/Departments
        public async Task<IActionResult> Departments()
        {
            // Veritabanındaki tüm bölümleri listeler
            var departments = await _context.Departments.ToListAsync();
            return View(departments);
        }

        // POST: Admin/CreateDepartment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDepartment(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                _context.Departments.Add(new Department { Name = name });
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Bölüm başarıyla eklendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Bölüm adı boş olamaz!";
            }
            return RedirectToAction(nameof(Departments));
        }
        // POST: Admin/DeleteDepartment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            // Bölümü ve içindeki doktorları kontrol et
            var department = await _context.Departments
                .Include(d => d.Doctors)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                TempData["ErrorMessage"] = "Bölüm bulunamadı.";
                return RedirectToAction(nameof(Departments));
            }

            // EĞER BÖLÜMDE DOKTOR VARSA SİLDİRME
            if (department.Doctors.Any())
            {
                TempData["ErrorMessage"] = $"Bu bölümde ({department.Name}) kayıtlı doktorlar bulunduğu için bölümü silemezsiniz. Önce doktorları silmeli veya başka bölüme taşımalısınız.";
                return RedirectToAction(nameof(Departments));
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bölüm başarıyla silindi.";
            return RedirectToAction(nameof(Departments));
        }
        // -----------------------------------------------------
        // HASTA YÖNETİMİ
        // -----------------------------------------------------

        // Tüm hastaları listeler
        public async Task<IActionResult> Patients()
        {
            var patients = await _context.Patients.ToListAsync();
            return View(patients);
        }

        // Bir hastayı siler
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Hasta kaydı sistemden kalıcı olarak silindi.";
            }
            return RedirectToAction(nameof(Patients));
        }
    }
}