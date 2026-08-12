using AestheticCenter.Core.Entities;
using AestheticCenter.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;

namespace AestheticCenter.Infrastructure;

public static class DbInitializer
{
    public static async Task Initialize(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // EnsureCreated es más robusto cuando mezclamos SQLite y Postgres al inicio
        await context.Database.EnsureCreatedAsync();

        await InicializarConfiguracionAsync(context);

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

    /// <summary>
    /// Crea la tabla de configuración y siembra la única fila.
    ///
    /// EnsureCreated crea la base entera cuando no existe, pero no toca una que
    /// ya tiene datos: sin este CREATE, la tabla nunca aparecería en producción
    /// y el sitio fallaría al leerla. La sintaxis funciona igual en SQLite y en
    /// PostgreSQL, que es lo que usa cada entorno.
    ///
    /// Va antes de la semilla de servicios a propósito: aquella corta con un
    /// return temprano si ya hay servicios cargados.
    /// </summary>
    private static async Task InicializarConfiguracionAsync(ApplicationDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "SiteSettings" (
                "Id" INTEGER NOT NULL PRIMARY KEY,
                "Phone" TEXT NOT NULL,
                "Address" TEXT NOT NULL
            )
            """);

        if (await context.SiteSettings.AnyAsync())
        {
            return;
        }

        // Los valores con los que arranca son los que el sitio tenía escritos en
        // el código, para que publicar este cambio no altere nada visible.
        context.SiteSettings.Add(new SiteSettings
        {
            Id = 1,
            Phone = "096 045 127",
            Address = "Coronel Lucas Píriz 2548",
        });
        await context.SaveChangesAsync();
    }
}
