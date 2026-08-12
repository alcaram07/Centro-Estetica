using AestheticCenter.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AestheticCenter.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Service> Services { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<SiteSettings> SiteSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configuración para que PostgreSQL acepte las fechas correctamente
        var dateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
            v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc));

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
            }
        }

        // Configuración adicional si es necesaria
        builder.Entity<Service>()
            .Property(s => s.Price)
            .HasPrecision(18, 2);

        // El nombre va explícito porque DbInitializer crea esta tabla por SQL y
        // los dos tienen que coincidir. La clave no es autogenerada: la fila es
        // siempre la 1.
        builder.Entity<SiteSettings>(e =>
        {
            e.ToTable("SiteSettings");
            e.Property(s => s.Id).ValueGeneratedNever();
        });
    }
}
