using System.ComponentModel.DataAnnotations;

namespace AestheticCenter.Core.Entities;

/// <summary>
/// Una reseña que deja una clienta desde /Resenas. Se guarda como no aprobada
/// y solo se muestra en la home cuando Patricia la aprueba desde el panel:
/// así el formulario público no puede publicar cualquier cosa directamente.
/// </summary>
public class Testimonial
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Escribí tu nombre.")]
    [StringLength(80)]
    public string ClientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contanos tu experiencia.")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Contanos un poco más (al menos 10 caracteres).")]
    public string Text { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Rating { get; set; } = 5;

    public DateTime CreatedAt { get; set; }

    public bool Approved { get; set; }
}
