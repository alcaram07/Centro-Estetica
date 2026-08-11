namespace AestheticCenter.Web;

/// <summary>
/// Datos de contacto y ubicación del negocio. Se usan tanto en el contenido
/// visible como en los datos estructurados (JSON-LD) del layout, para que
/// ambos no se desincronicen.
///
/// Dirección y Horarios están vacíos a propósito: cuando lo están, el sitio
/// muestra una alternativa ("consultar por WhatsApp") y los omite del JSON-LD,
/// en lugar de publicar datos inventados o incompletos, que Google penaliza.
/// Al completarlos, el texto y los datos estructurados se actualizan solos.
/// </summary>
public static class SiteInfo
{
    public static readonly string Url = "https://centro-estetica.onrender.com";

    /// <summary>Formato internacional, requerido por schema.org.</summary>
    public static readonly string Telefono = "+59896045127";

    /// <summary>Formato local, para mostrar en pantalla.</summary>
    public static readonly string TelefonoVisible = "096 045 127";

    public static readonly string WhatsApp = "https://wa.me/598096045127";

    public static readonly string Barrio = "La Blanqueada";
    public static readonly string Ciudad = "Montevideo";
    public static readonly string CodigoPostal = "11600";

    /// <summary>Calle y número.</summary>
    public static readonly string Direccion = "Coronel Lucas Píriz 2548";

    /// <summary>Texto de los horarios, tal como se muestra en pantalla.</summary>
    public static readonly string Horarios = "Lunes a viernes de 9:00 a 19:00";

    // Los mismos horarios en el formato que exige schema.org, para que Google
    // pueda mostrar "Abierto · Cierra a las 19:00" en los resultados locales.
    public static readonly string[] DiasAtencion =
        { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
    public static readonly string HoraApertura = "09:00";
    public static readonly string HoraCierre = "19:00";

    /// <summary>Los días de atención como lista JSON, para incrustar en el JSON-LD.</summary>
    public static string DiasAtencionJson => "\"" + string.Join("\", \"", DiasAtencion) + "\"";

    public static bool TieneDireccion => !string.IsNullOrWhiteSpace(Direccion);

    /// <summary>Enlace a Google Maps con la dirección completa, para "Cómo llegar".</summary>
    public static string MapaUrl =>
        "https://www.google.com/maps/search/?api=1&query=" +
        System.Net.WebUtility.UrlEncode($"{Direccion}, {Barrio}, {Ciudad}, Uruguay");
    public static bool TieneHorarios => !string.IsNullOrWhiteSpace(Horarios);
}
