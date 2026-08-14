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
        await AgregarColumnaTextoLargoAsync(context);
        await AgregarColumnaFechaAsync(context);
        await InicializarResenasAsync(context);

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

        // 3. Promover a Patricia a Admin. Se registró ella misma desde
        // /Identity/Account/Register (así su contraseña no pasa por acá ni por
        // git); esto solo la suma al rol. Paso temporal: se saca del código
        // una vez confirmado que quedó con acceso al panel.
        var patriciaEmail = "patocaram@hotmail.es";
        var patriciaUser = await userManager.FindByEmailAsync(patriciaEmail);
        if (patriciaUser != null && !await userManager.IsInRoleAsync(patriciaUser, "Admin"))
        {
            await userManager.AddToRoleAsync(patriciaUser, "Admin");
        }

        // 4. Semilla de Servicios
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

    /// <summary>
    /// Agrega Services.LongDescription si todavía no está.
    ///
    /// Mismo problema que la tabla de configuración: sin migraciones,
    /// EnsureCreated no modifica una base que ya existe. Acá no sirve un
    /// "IF NOT EXISTS" porque SQLite no lo acepta en ADD COLUMN.
    /// </summary>
    private static async Task AgregarColumnaTextoLargoAsync(ApplicationDbContext context)
    {
        if (await ExisteColumnaAsync(context, "Services", "LongDescription"))
        {
            return;
        }

        await context.Database.ExecuteSqlRawAsync(
            """ALTER TABLE "Services" ADD COLUMN "LongDescription" TEXT NOT NULL DEFAULT ''""");
    }

    /// <summary>
    /// Agrega Services.UpdatedAt si todavía no está: es la fecha que el sitemap
    /// declara como lastmod.
    ///
    /// A diferencia de las otras columnas, acá el tipo sí depende del motor.
    /// Queda nullable a propósito, para no inventarle una fecha de modificación
    /// a los servicios que ya existían.
    /// </summary>
    private static async Task AgregarColumnaFechaAsync(ApplicationDbContext context)
    {
        if (await ExisteColumnaAsync(context, "Services", "UpdatedAt"))
        {
            return;
        }

        // Dos sentencias completas en lugar de interpolar el tipo: el tipo de una
        // columna no puede viajar como parámetro, y así no queda SQL armado por
        // concatenación.
        var sql = context.Database.IsNpgsql()
            ? """ALTER TABLE "Services" ADD COLUMN "UpdatedAt" timestamp with time zone NULL"""
            : """ALTER TABLE "Services" ADD COLUMN "UpdatedAt" TEXT NULL""";

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    /// <summary>
    /// Crea la tabla de reseñas. Mismo motivo que la de configuración: es una
    /// tabla nueva y EnsureCreated no la agrega a una base que ya existía.
    ///
    /// El tipo de "CreatedAt" y "Approved" sí depende del motor, como en
    /// Services.UpdatedAt: SQLite los guarda como texto y entero, Postgres
    /// tiene tipos nativos para fecha con huso horario y booleano.
    /// </summary>
    private static async Task InicializarResenasAsync(ApplicationDbContext context)
    {
        var sql = context.Database.IsNpgsql()
            ? """
              CREATE TABLE IF NOT EXISTS "Testimonials" (
                  "Id" SERIAL NOT NULL PRIMARY KEY,
                  "ClientName" TEXT NOT NULL,
                  "Text" TEXT NOT NULL,
                  "Rating" INTEGER NOT NULL,
                  "CreatedAt" timestamp with time zone NOT NULL,
                  "Approved" BOOLEAN NOT NULL DEFAULT FALSE
              )
              """
            : """
              CREATE TABLE IF NOT EXISTS "Testimonials" (
                  "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                  "ClientName" TEXT NOT NULL,
                  "Text" TEXT NOT NULL,
                  "Rating" INTEGER NOT NULL,
                  "CreatedAt" TEXT NOT NULL,
                  "Approved" INTEGER NOT NULL DEFAULT 0
              )
              """;

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    /// <summary>
    /// Si una columna existe, preguntándole al motor por las columnas que
    /// devuelve la tabla.
    ///
    /// Se lee del reader en vez de consultar el catálogo del motor (PRAGMA en
    /// SQLite, information_schema en PostgreSQL) para no escribir una consulta
    /// por dialecto. Tampoco sirve intentar un SELECT de la columna y esperar
    /// que falle: ExecuteSqlRaw lo da por ejecutado sin evaluarlo, y la
    /// excepción nunca llega.
    /// </summary>
    private static async Task<bool> ExisteColumnaAsync(
        ApplicationDbContext context, string tabla, string columna)
    {
        var conexion = context.Database.GetDbConnection();
        var estabaCerrada = conexion.State != System.Data.ConnectionState.Open;

        if (estabaCerrada)
        {
            await conexion.OpenAsync();
        }

        try
        {
            using var comando = conexion.CreateCommand();
            comando.CommandText = $"""SELECT * FROM "{tabla}" WHERE 1 = 0""";

            using var reader = await comando.ExecuteReaderAsync();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (string.Equals(reader.GetName(i), columna, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        finally
        {
            if (estabaCerrada)
            {
                await conexion.CloseAsync();
            }
        }
    }
}
