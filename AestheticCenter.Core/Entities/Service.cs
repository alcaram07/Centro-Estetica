using System.ComponentModel.DataAnnotations;

namespace AestheticCenter.Core.Entities;

public class Service
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Una frase, para la tarjeta del listado.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// El texto de la página propia del tratamiento (/Services/{slug}).
    /// Mientras esté vacío la página existe pero se marca como noindex y no
    /// entra al sitemap: una página con dos líneas no la posiciona Google y
    /// resta más de lo que suma.
    /// </summary>
    /// <remarks>
    /// Los dos atributos son para que el campo pueda quedar vacío, que es lo
    /// normal hasta que se escriba el texto. Sin ellos, el alta y la edición de
    /// servicios fallaban con "The LongDescription field is required":
    ///
    /// - El proyecto compila con referencias nulables activadas y ASP.NET da
    ///   por obligatoria toda propiedad string que no sea nulable, así que hace
    ///   falta un Required propio que admita texto vacío.
    /// - Aun así seguía fallando, porque el enlazador convierte la cadena vacía
    ///   en null antes de validar y la validación la rechazaba igual.
    ///
    /// Se resuelve por acá y no haciendo la columna nulable, que obligaría a
    /// tocar el esquema en producción.
    /// </remarks>
    [Required(AllowEmptyStrings = true)]
    [DisplayFormat(ConvertEmptyStringToNull = false)]
    public string LongDescription { get; set; } = string.Empty;

    /// <summary>
    /// Cuándo se guardó por última vez, para declararlo como lastmod en el
    /// sitemap y que Google sepa si hay algo nuevo que rastrear.
    ///
    /// Admite nulo porque los servicios anteriores a este campo no tienen una
    /// fecha real, y es preferible omitir el dato antes que inventarlo: un
    /// lastmod poco confiable termina ignorado.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    [Range(0.01, 10000)]
    public decimal Price { get; set; }

    public int DurationMinutes { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
