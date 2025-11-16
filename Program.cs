using HastaneRandevuSistemi.Data;
using Microsoft.EntityFrameworkCore;
using HastaneRandevuSistemi.Services; // === YENÝ EKLENDÝ === (Servisler için)
using Microsoft.AspNetCore.Authentication.Cookies; // === YENÝ EKLENDÝ === (Cookie Auth için)

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// SESSION EKLE
builder.Services.AddSession();

// Veritabaný baðlantýsý
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================================
// === YENÝ EKLENDÝ: DEPENDENCY INJECTION (Servislerin Tanýtýlmasý) ===
// ============================================================
// Controller, IAuthService istediðinde AuthService ver:
builder.Services.AddScoped<IAuthService, AuthService>();

// Controller, IAppointmentService istediðinde AppointmentService ver:
builder.Services.AddScoped<IAppointmentService, AppointmentService>();


// ============================================================
// === YENÝ EKLENDÝ: AUTHENTICATION (Kimlik Doðrulama Ayarlarý) ===
// ============================================================
// Giriþ yapma sistemi için Cookie kullanacaðýmýzý belirtiyoruz.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Kullanýcý giriþ yapmadan [Authorize] olan bir sayfaya girmek isterse 
        // buraya yönlendirilsin:
        options.LoginPath = "/Account/Login";

        // Yetkisi olmayan bir yere girmek isterse:
        options.AccessDeniedPath = "/Account/AccessDenied";

        // Cookie ömrü (30 dakika hareketsiz kalýrsa çýkýþ yap)
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        // Kullanýcý her týkladýðýnda süreyi uzat:
        options.SlidingExpiration = true;
    });


var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// SESSION ORTAYA EKLENÝYOR
app.UseSession();

// DÝKKAT: Bu sýralama çok önemlidir!
// (Mevcut kodunuzdaki sýralama zaten doðruydu)
app.UseAuthentication(); // Önce "Sen kimsin?" (Kimlik Doðrulama)
app.UseAuthorization();  // Sonra "Buna yetkin var mý?" (Yetkilendirme)

// Route ayarý
// Not: Varsayýlan rotayý "Account/Login" yapmak, giriþ yapan kullanýcýlar için 
// sonsuz döngüye neden olabilir. En iyisi ana sayfayý "Home" yapmak
// ve [Authorize] attribute'unun kullanýcýyý Login'e yönlendirmesine izin vermektir.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); // "Patient/Login" yerine "Home/Index" olarak deðiþtirdim.

app.Run();