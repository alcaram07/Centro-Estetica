using System.ComponentModel.DataAnnotations;

namespace AestheticCenter.Core.Entities;

/// <summary>
/// Datos de contacto que Patricia puede cambiar sin tocar el código. Es una
/// tabla de una sola fila (Id = 1): no hay varios sitios que configurar.
///
/// El teléfono se guarda tal como se escribe en Uruguay ("096 045 127"); los
/// formatos internacional, de WhatsApp y de pantalla se derivan de ahí, para
/// que no puedan quedar desincronizados entre sí.
/// </summary>
public class SiteSettings
{
    /// <summary>Siempre 1. La tabla existe para tener una fila editable, no una lista.</summary>
    public int Id { get; set; }

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [StringLength(30)]
    [Display(Name = "Teléfono / WhatsApp")]
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Puede quedar vacía: el sitio muestra "dirección por WhatsApp" y la omite
    /// del JSON-LD, en lugar de publicar datos incompletos.
    /// </summary>
    [StringLength(200)]
    [Display(Name = "Dirección")]
    public string Address { get; set; } = string.Empty;
}
