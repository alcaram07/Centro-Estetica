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
    public string LongDescription { get; set; } = string.Empty;

    [Range(0.01, 10000)]
    public decimal Price { get; set; }

    public int DurationMinutes { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
