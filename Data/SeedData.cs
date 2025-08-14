using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemApp.Models;

namespace SistemApp.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Kullanıcı yöneticisi ve rol yöneticisi oluştur
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Roller yoksa oluştur
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                if (!await roleManager.RoleExistsAsync("User"))
                {
                    await roleManager.CreateAsync(new IdentityRole("User"));
                }

                // Admin kullanıcısı yoksa oluştur
                string adminEmail = "admin@sistem.com";
                string adminUserName = "admin";
                string adminPassword = "Admin123!";

                if (await userManager.FindByNameAsync(adminUserName) == null)
                {
                    var admin = new ApplicationUser
                    {
                        UserName = adminUserName,
                        Email = adminEmail,
                        FirstName = "Admin",
                        LastName = "User",
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(admin, adminPassword);

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(admin, "Admin");
                    }
                }

                // Normal kullanıcı yoksa oluştur
                string userEmail = "user@sistem.com";
                string userName = "user";
                string userPassword = "User123!";

                if (await userManager.FindByNameAsync(userName) == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = userName,
                        Email = userEmail,
                        FirstName = "Normal",
                        LastName = "User",
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user, userPassword);

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "User");
                    }
                }
            }
        }
    }
}