using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AestheticCenter.Web.Pages;

/// <summary>
/// El sitemap se genera en vez de ser un archivo fijo porque las páginas de los
/// tratamientos aparecen y desaparecen según tengan texto cargado. Solo se
/// listan las que se quieren indexadas: declarar una página que además lleva
/// noindex son señales contradictorias.
/// </summary>
public class SitemapModel(CatalogoServicios catalogo) : PageModel
{
    public IActionResult OnGet()
    {
        var hoy = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var sb = new StringBuilder();

        sb.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
        sb.AppendLine("""<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">""");

        Agregar(sb, "/", "1.0", hoy);
        Agregar(sb, "/Services", "0.8", hoy);
        Agregar(sb, "/Gallery", "0.8", hoy);

        foreach (var servicio in catalogo.ConPagina)
        {
            Agregar(sb, $"/Services/{servicio.Slug}", "0.7", hoy);
        }

        sb.AppendLine("</urlset>");

        return Content(sb.ToString(), "application/xml", Encoding.UTF8);
    }

    private static void Agregar(StringBuilder sb, string ruta, string prioridad, string fecha)
    {
        // Los slugs son [a-z0-9-] y las rutas son fijas, pero se escapa igual:
        // los nombres de los tratamientos los carga una persona y el sitemap
        // deja de ser XML válido con un solo "&".
        var url = XmlEscape(SiteInfo.Url + ruta);

        sb.AppendLine("  <url>");
        sb.AppendLine($"    <loc>{url}</loc>");
        sb.AppendLine($"    <lastmod>{fecha}</lastmod>");
        sb.AppendLine("    <changefreq>weekly</changefreq>");
        sb.AppendLine($"    <priority>{prioridad}</priority>");
        sb.AppendLine("  </url>");
    }

    private static string XmlEscape(string valor)
    {
        var sb = new StringBuilder();
        using var writer = XmlWriter.Create(sb, new XmlWriterSettings
        {
            ConformanceLevel = ConformanceLevel.Fragment,
            OmitXmlDeclaration = true,
        });
        writer.WriteString(valor);
        writer.Flush();
        return sb.ToString();
    }
}
