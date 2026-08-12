using AestheticCenter.Core.Entities;
using AestheticCenter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AestheticCenter.Web;

/// <summary>
/// Acceso a los datos de contacto editables. Es la contraparte dinámica de
/// <see cref="SiteInfo"/>: expone las mismas propiedades que antes eran
/// constantes, pero leyéndolas de la base.
///
/// Se registra como singleton y cachea la fila en memoria porque el layout la
/// usa en todas las páginas y no tiene sentido consultarla en cada request;
/// <see cref="GuardarAsync"/> invalida la caché al escribir.
/// </summary>
public class SiteSettingsProvider(IServiceScopeFactory scopeFactory)
{
    private readonly object _candado = new();
    private SiteSettings? _cache;

    /// <summary>
    /// Si la tabla todavía no existe o está vacía (primer arranque, antes de que
    /// corra el inicializador), se devuelven estos valores en lugar de romper la
    /// página: el sitio sigue en pie aunque la configuración no cargue.
    /// </summary>
    private static readonly SiteSettings Respaldo = new()
    {
        Id = 1,
        Phone = "096 045 127",
        Address = "",
    };

    public SiteSettings Actual
    {
        get
        {
            lock (_candado)
            {
                return _cache ??= Cargar();
            }
        }
    }

    private SiteSettings Cargar()
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return db.SiteSettings.AsNoTracking().FirstOrDefault() ?? Respaldo;
        }
        catch (Exception)
        {
            return Respaldo;
        }
    }

    public async Task GuardarAsync(string telefono, string direccion)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var actual = await db.SiteSettings.FirstOrDefaultAsync();
        if (actual is null)
        {
            actual = new SiteSettings { Id = 1 };
            db.SiteSettings.Add(actual);
        }

        actual.Phone = telefono.Trim();
        actual.Address = direccion.Trim();
        await db.SaveChangesAsync();

        lock (_candado)
        {
            _cache = null;
        }
    }

    // ── Formatos derivados del teléfono ──────────────────────────────
    //
    // El número se guarda como se escribe acá ("096 045 127") y de ahí salen
    // los tres formatos que usa el sitio. Antes cada uno estaba escrito a mano
    // y el de WhatsApp tenía un dígito de más.

    /// <summary>Los dígitos del número sin el 0 inicial ni el prefijo de país: "96045127".</summary>
    private string Nacional
    {
        get
        {
            var digitos = new string(Actual.Phone.Where(char.IsDigit).ToArray());
            if (digitos.StartsWith("598"))
            {
                digitos = digitos[3..];
            }
            return digitos.TrimStart('0');
        }
    }

    /// <summary>Formato internacional, requerido por schema.org: "+59896045127".</summary>
    public string Telefono => "+598" + Nacional;

    /// <summary>Formato local, para mostrar en pantalla: "096 045 127".</summary>
    public string TelefonoVisible
    {
        get
        {
            var n = Nacional;
            // Celulares uruguayos: 8 dígitos que se agrupan 0XX XXX XXX. Otros
            // largos (fijos, números mal cargados) se muestran tal cual quedaron.
            return n.Length == 8
                ? $"0{n[..2]} {n[2..5]} {n[5..]}"
                : "0" + n;
        }
    }

    /// <summary>
    /// Enlace a WhatsApp. El país va sin el "+" y el número sin el 0 inicial:
    /// wa.me rechaza el número si se le deja ese cero.
    /// </summary>
    public string WhatsApp => "https://wa.me/598" + Nacional;

    /// <summary>Enlace a WhatsApp con un mensaje ya escrito.</summary>
    public string WhatsAppCon(string mensaje) => $"{WhatsApp}?text={Uri.EscapeDataString(mensaje)}";

    public string Direccion => Actual.Address;

    public bool TieneDireccion => !string.IsNullOrWhiteSpace(Direccion);

    /// <summary>Enlace a Google Maps con la dirección completa, para "Cómo llegar".</summary>
    public string MapaUrl =>
        "https://www.google.com/maps/search/?api=1&query=" +
        System.Net.WebUtility.UrlEncode($"{Direccion}, {SiteInfo.Barrio}, {SiteInfo.Ciudad}, Uruguay");
}
