using AestheticCenter.Core.Entities;
using AestheticCenter.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace AestheticCenter.Infrastructure;

public static class DbInitializer
{
    public static async Task Initialize(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        context.Database.EnsureCreated();

        // 1. Crear Roles si no existen
        string[] roleNames = { "Admin", "Customer" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 2. Crear Usuario Admin por defecto
        var adminEmail = "admin@aestheticcenter.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(adminUser, "Admin123!");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // 3. Semilla de Servicios
        if (context.Services.Any())
        {
            return;
        }

        var services = new Service[]
        {
            new Service { Name = "Limpieza Facial Profunda", Description = "Tratamiento completo para eliminar impurezas y renovar la piel.", Price = 45.00m, DurationMinutes = 60 },
            new Service { Name = "Masaje Relajante", Description = "Masaje corporal con aceites esenciales para aliviar el estrés.", Price = 35.00m, DurationMinutes = 45 },
            new Service { Name = "Manicura y Pedicura", Description = "Cuidado completo de manos y pies con esmaltado de larga duración.", Price = 25.00m, DurationMinutes = 90 },
            new Service { Name = "Tratamiento Anti-Edad", Description = "Procedimiento avanzado con colágeno para reducir líneas de expresión.", Price = 65.00m, DurationMinutes = 75 }
        };

        context.Services.AddRange(services);
        await context.SaveChangesAsync();
    }
}
