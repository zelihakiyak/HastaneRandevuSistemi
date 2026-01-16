using HastaneRandevuSistemi.Data;
using HastaneRandevuSistemi.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.WebEncoders;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------
// MVC + API Controller Desteği
// ------------------------------------------------------
builder.Services.AddControllersWithViews();

// API endpoint’leri için zorunlu
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ------------------------------------------------------
// SESSION EKLE
// ------------------------------------------------------
builder.Services.AddSession();

// ------------------------------------------------------
// Veritabanı Bağlantısı
// ------------------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// ------------------------------------------------------
// DEPENDENCY INJECTION (Servis Tanımları)
// ------------------------------------------------------
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();


// ------------------------------------------------------
// AUTHENTICATION (Cookie Ayarları)
// ------------------------------------------------------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(30); // 30 gün boyunca hatırla
        options.SlidingExpiration = true; // Kullanıcı siteyi kullandıkça süre uzasın
        options.Cookie.Name = "HealthConnectAuth";
    });

builder.Services.Configure<WebEncoderOptions>(options =>
{
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
});

var app = builder.Build();


// ------------------------------------------------------
// SWAGGER UI (Geliştirme Ortamında Açık)
// ------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


// ------------------------------------------------------
// STANDART MIDDLEWARE SIRALAMASI
// ------------------------------------------------------
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session → Authentication → Authorization sırası doğru
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();


// ------------------------------------------------------
// ROUTING
// ------------------------------------------------------

// MVC Controller route (View’ler için)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// API Controller route (Swagger buradaki endpointleri görür)
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    // Seed işlemini başlat
    await DbSeeder.SeedAdminAsync(context);
}
app.Run();


