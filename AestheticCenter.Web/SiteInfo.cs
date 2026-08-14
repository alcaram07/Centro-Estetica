namespace AestheticCenter.Web;

/// <summary>
/// Datos del negocio que no cambian salvo que se mude o cambie de horarios. Se
/// usan tanto en el contenido visible como en los datos estructurados (JSON-LD)
/// del layout, para que ambos no se desincronicen.
///
/// El teléfono y la dirección NO están acá: se editan desde /Admin/Settings y
/// se leen con <see cref="SiteSettingsProvider"/>.
///
/// Horarios puede quedar vacío: cuando lo está, el sitio lo omite del JSON-LD
/// en lugar de publicar datos incompletos, que Google penaliza.
/// </summary>
public static class SiteInfo
{
    /// <summary>
    /// De acá salen el canonical de todas las páginas, el sitemap, el JSON-LD y
    /// las etiquetas para redes. Si vuelve a cambiar, actualizar también la
    /// línea Sitemap de wwwroot/robots.txt, que lo lleva escrito.
    /// </summary>
    public static readonly string Url = "https://paticestetica.com";

    /// <summary>
    /// La reserva de turnos online. Apagada: el sitio funciona como landing y
    /// los turnos se coordinan por WhatsApp, con el tratamiento ya escrito en el
    /// mensaje.
    ///
    /// El módulo de citas quedó entero (páginas, base y panel). Para volver a
    /// activarlo hay que poner esto en true y devolver los enlaces a
    /// /Appointments/Book en Services/Index.cshtml y Services/Detail.cshtml.
    ///
    /// Es "static readonly" y no "const" a propósito: como const, el compilador
    /// resolvería las condiciones y marcaría el resto del flujo como código
    /// inalcanzable.
    /// </summary>
    public static readonly bool ReservaOnlineActiva = false;

    public static readonly string Barrio = "La Blanqueada";
    public static readonly string Ciudad = "Montevideo";
    public static readonly string CodigoPostal = "11600";

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

    public static bool TieneHorarios => !string.IsNullOrWhiteSpace(Horarios);

    /// <summary>
    /// Precio en pesos uruguayos. El proyecto compila con InvariantGlobalization,
    /// asi que ToString("C") saldria con el simbolo generico de moneda ("¤") y
    /// separadores anglosajones; de ahi que el formato se arme a mano.
    /// </summary>
    public static string FormatoPrecio(decimal precio) =>
        "$ " + precio.ToString(precio == decimal.Truncate(precio) ? "#,0" : "#,0.00", PesoUruguayo);

    private static readonly System.Globalization.NumberFormatInfo PesoUruguayo = new()
    {
        NumberGroupSeparator = ".",
        NumberDecimalSeparator = ",",
    };
}
