using AestheticCenter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AestheticCenter.Web;

/// <summary>
/// Los nombres de los servicios, para los datos estructurados y los textos que
/// lee Google. Antes el catálogo del JSON-LD estaba escrito a mano en el layout
/// y había quedado desfasado de los servicios que realmente se ofrecen.
///
/// Cachea unos minutos porque el layout lo usa en todas las páginas. No hay
/// invalidación explícita al editar un servicio: el desfasaje dura lo que la
/// caché y no vale acoplar las tres páginas del panel a esto.
/// </summary>
public class CatalogoServicios(IServiceScopeFactory scopeFactory)
{
    private static readonly TimeSpan Vigencia = TimeSpan.FromMinutes(10);

    private readonly object _candado = new();
    private IReadOnlyList<string>? _cache;
    private DateTime _cargado = DateTime.MinValue;

    public IReadOnlyList<string> Nombres
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

    private IReadOnlyList<string> Cargar()
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return db.Services
                .AsNoTracking()
                .OrderBy(s => s.Id)
                .Select(s => s.Name)
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
        var nombres = Nombres;
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
