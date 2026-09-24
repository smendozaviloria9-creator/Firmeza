using Firmeza.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Web.Data;


public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Requisito: la estructura de la BD se construye solo con migraciones EF Core.
        await context.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var roleName in new[] { Roles.Administrador, Roles.Cliente })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        var adminEmail = configuration["AdminSeed:Email"] ?? "admin@firmeza.com";
        var adminPassword = configuration["AdminSeed:Password"] ?? "Admin123$";
        var adminNombre = configuration["AdminSeed:NombreCompleto"] ?? "Administrador Firmeza";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                NombreCompleto = adminNombre
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, Roles.Administrador);
            }
        }
    }
}
