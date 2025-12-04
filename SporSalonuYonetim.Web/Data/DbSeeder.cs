using Microsoft.AspNetCore.Identity;
using SporSalonuYonetim.Web.Entities;

namespace SporSalonuYonetim.Web.Data
{
    public static class DbSeeder
    {
        public static async Task SeedData(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<AppDbContext>();

            // 1. ROLLERİ OLUŞTUR
            string[] roleNames = { "Admin", "Member" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. ADMIN KULLANICISINI OLUŞTUR
            // Senin numaranla güncellendi:
            var adminEmail = "g221210083@sakarya.edu.tr"; 
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                
                // Şifre: sau (Program.cs'te ayar yapmazsak bu hata verir!)
                var createAdmin = await userManager.CreateAsync(newAdmin, "sau"); 
                
                if (createAdmin.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }

            // 3. ÖRNEK HİZMETLER
            if (!context.Services.Any())
            {
                context.Services.AddRange(
                    new Service { Name = "Fitness", Duration = 60, Price = 500, Description = "Birebir antrenör eşliğinde fitness." },
                    new Service { Name = "Yoga", Duration = 45, Price = 300, Description = "Zihin ve beden rahatlaması için yoga." },
                    new Service { Name = "Pilates", Duration = 50, Price = 400, Description = "Aletli reformer pilates dersi." }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}