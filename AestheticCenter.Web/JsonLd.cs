using System.Text.Json;

namespace AestheticCenter.Web;

public static class JsonLd
{
    /// <summary>
    /// Prepara un texto para incrustarlo entre comillas dentro del bloque
    /// JSON-LD del layout.
    ///
    /// Hace falta porque Razor escapa su salida como HTML ("+" queda "&#x2B;",
    /// "í" queda "&#xED;"), y dentro de un &lt;script&gt; esas entidades no se
    /// decodifican: Google terminaba leyendo el teléfono y la dirección
    /// literalmente con las entidades adentro. Se usa junto con Html.Raw, que
    /// evita ese escapado de Razor.
    ///
    /// Escapa además las comillas, con lo cual una dirección cargada desde el
    /// panel no puede romper el JSON. El codificador por defecto convierte
    /// "&lt;" en "<", así que un texto con "&lt;/script&gt;" tampoco puede
    /// cerrar el bloque antes de tiempo.
    /// </summary>
    public static string Texto(string? valor) =>
        JsonEncodedText.Encode(valor ?? string.Empty).ToString();
}
