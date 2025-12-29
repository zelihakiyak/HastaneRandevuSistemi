using HastaneRandevuSistemi.Models;
using HastaneRandevuSistemi.Data;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevuSistemi.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAdminAsync(ApplicationDbContext context)
        {
            // Veritabanında herhangi bir admin var mı kontrol et
            if (!await context.Admins.AnyAsync())
            {
                var defaultAdmin = new Admin
                {
                    FullName = "Sistem Yöneticisi",
                    Email = "admin@hastane.com",
                    // Şifreyi BCrypt ile güvenli hale getiriyoruz
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!")
                };

                context.Admins.Add(defaultAdmin);
                await context.SaveChangesAsync();
            }
        }
    }
}