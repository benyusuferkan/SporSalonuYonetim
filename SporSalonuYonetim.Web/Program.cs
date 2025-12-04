using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetim.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı Bağlantısı Ayarı
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Üyelik (Identity) Sistemi Ayarı
// DİKKAT: "sau" şifresini kabul etmesi için kuralları gevşetiyoruz
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false; // Sayı zorunluluğunu kaldır
    options.Password.RequireLowercase = false; // Küçük harf zorunluluğunu kaldır
    options.Password.RequireUppercase = false; // Büyük harf zorunluluğunu kaldır
    options.Password.RequireNonAlphanumeric = false; // Sembol (!,*,.) zorunluluğunu kaldır
    options.Password.RequiredLength = 3; // En az 3 karakter olsun ("sau" 3 harf)
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// --- SEED DATA KISMI ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbSeeder.SeedData(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı verileri yüklenirken bir hata oluştu.");
    }
}
// --- SEED DATA BİTİŞ ---

app.Run(); // Tek ve son satır burası olacak!