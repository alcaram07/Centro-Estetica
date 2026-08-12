using AestheticCenter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AestheticCenter.Web;

/// <summary>Un servicio, con lo que hace falta para armar sus enlaces y su SEO.</summary>
/// <param name="Nombre">Como se muestra y como se le declara a Google.</param>
/// <param name="Slug">La parte final de la URL de su página propia.</param>
/// <param name="TienePagina">
/// Si tiene texto largo cargado. Sin él la página existe pero va con noindex y
/// queda fuera del sitemap.
/// </param>
public record ServicioSeo(string Nombre, string Slug, bool TienePagina);

/// <summary>
/// Los servicios, para los datos estructurados, los enlaces y los textos que lee
/// Google. Antes el catálogo del JSON-LD estaba escrito a mano en el layout y
/// había quedado desfasado de los servicios que realmente se ofrecen.
///
/// Cachea unos minutos porque el layout lo usa en todas las páginas. No hay
/// invalidación explícita al editar un servicio: el desfasaje dura lo que la
/// caché y no vale acoplar las tres páginas del panel a esto.
/// </summary>
public class CatalogoServicios(IServiceScopeFactory scopeFactory)
{
    private static readonly TimeSpan Vigencia = TimeSpan.FromMinutes(10);

    private readonly object _candado = new();
    private IReadOnlyList<ServicioSeo>? _cache;
    private DateTime _cargado = DateTime.MinValue;

    public IReadOnlyList<ServicioSeo> Servicios
    {
        get
        {
            lock (_candado)
            {
                if (_cache is null || DateTime.UtcNow - _cargado > Vigencia)
                {
                    _cache = Cargar();
                    _cargado = DateTime.UtcNow;
                }
                return _cache;
            }
        }
    }

    public IEnumerable<string> Nombres => Servicios.Select(s => s.Nombre);

    /// <summary>Solo los que tienen texto: son los únicos que se le ofrecen a Google.</summary>
    public IEnumerable<ServicioSeo> ConPagina => Servicios.Where(s => s.TienePagina);

    private IReadOnlyList<ServicioSeo> Cargar()
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return db.Services
                .AsNoTracking()
                .OrderBy(s => s.Id)
                .Select(s => new { s.Name, s.LongDescription })
                .AsEnumerable()
                .Select(s => new ServicioSeo(
                    s.Name,
                    Slug.Desde(s.Name),
                    !string.IsNullOrWhiteSpace(s.LongDescription)))
                .ToList();
        }
        catch (Exception)
        {
            // Si la consulta falla, el JSON-LD sale sin catálogo en lugar de
            // tumbar todas las páginas del sitio.
            return [];
        }
    }

    /// <summary>
    /// El catálogo como lista de ofertas de schema.org, listo para incrustar
    /// dentro del bloque JSON-LD del layout.
    /// </summary>
    public string OfertasJson()
    {
        var ofertas = Nombres.Select(n =>
            $$"""{ "@type": "Offer", "itemOffered": { "@type": "Service", "name": "{{JsonLd.Texto(n)}}" } }""");
        return string.Join(",\n          ", ofertas);
    }

    /// <summary>Los nombres en una frase, para las descripciones de las páginas.</summary>
    public string EnTexto()
    {
        var nombres = Nombres.ToList();
        if (nombres.Count == 0)
        {
            return string.Empty;
        }
        if (nombres.Count == 1)
        {
            return nombres[0];
        }
        return string.Join(", ", nombres.Take(nombres.Count - 1)) + " y " + nombres[^1];
    }
}
