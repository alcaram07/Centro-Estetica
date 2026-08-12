using System.Text;

namespace AestheticCenter.Web;

/// <summary>
/// Convierte el nombre de un tratamiento en la parte final de su URL:
/// "Limpieza Facial Profunda" queda como "limpieza-facial-profunda".
///
/// El slug se calcula a partir del nombre en vez de guardarse en una columna:
/// son cuatro servicios, se comparan en memoria y evita otro cambio de esquema
/// en un proyecto que no tiene migraciones.
/// </summary>
public static class Slug
{
    // Las tildes se sacan con una tabla explícita y no con string.Normalize:
    // el proyecto compila con InvariantGlobalization y la normalización Unicode
    // no es confiable en ese modo.
    private static readonly Dictionary<char, char> SinTilde = new()
    {
        ['á'] = 'a', ['é'] = 'e', ['í'] = 'i', ['ó'] = 'o', ['ú'] = 'u',
        ['à'] = 'a', ['è'] = 'e', ['ì'] = 'i', ['ò'] = 'o', ['ù'] = 'u',
        ['ä'] = 'a', ['ë'] = 'e', ['ï'] = 'i', ['ö'] = 'o', ['ü'] = 'u',
        ['â'] = 'a', ['ê'] = 'e', ['î'] = 'i', ['ô'] = 'o', ['û'] = 'u',
        ['ñ'] = 'n', ['ç'] = 'c',
    };

    public static string Desde(string texto)
    {
        var sb = new StringBuilder(texto.Length);
        var guionPendiente = false;

        foreach (var c in texto.ToLowerInvariant())
        {
            var letra = SinTilde.TryGetValue(c, out var reemplazo) ? reemplazo : c;

            if (letra is >= 'a' and <= 'z' || letra is >= '0' and <= '9')
            {
                // El guión se agrega recién cuando hay algo después, así no
                // quedan guiones al final ni repetidos en el medio.
                if (guionPendiente && sb.Length > 0)
                {
                    sb.Append('-');
                }
                guionPendiente = false;
                sb.Append(letra);
            }
            else
            {
                guionPendiente = true;
            }
        }

        return sb.ToString();
    }
}
