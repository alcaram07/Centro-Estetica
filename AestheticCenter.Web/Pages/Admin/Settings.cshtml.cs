using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AestheticCenter.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class SettingsModel(SiteSettingsProvider settings) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>Los formatos derivados del teléfono, para mostrar cómo queda al guardarlo.</summary>
    public string VistaPreviaWhatsApp { get; private set; } = string.Empty;
    public string VistaPreviaVisible { get; private set; } = string.Empty;

    /// <summary>
    /// Al guardar se redirige en lugar de devolver la página, para que recargar
    /// no reenvíe el formulario. El aviso viaja en la URL y no por TempData,
    /// que depende de una cookie y acá no llegaba a destino.
    /// </summary>
    public bool Guardado { get; private set; }

    public class InputModel
    {
        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(@"^[\d\s\+\-\(\)]{8,20}$",
            ErrorMessage = "Escribí solo números, con o sin espacios. Ejemplo: 096 045 127")]
        [Display(Name = "Teléfono / WhatsApp")]
        public string Telefono { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La dirección no puede pasar de 200 caracteres.")]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; } = string.Empty;
    }

    public void OnGet(bool guardado = false)
    {
        Guardado = guardado;
        Input = new InputModel
        {
            Telefono = settings.Actual.Phone,
            Direccion = settings.Actual.Address,
        };
        CargarVistaPrevia();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            CargarVistaPrevia();
            return Page();
        }

        // Sin dígitos no se puede armar ningún enlace: la expresión regular deja
        // pasar cadenas como "+ - ( )", que serían válidas de forma y sin número.
        if (!Input.Telefono.Any(char.IsDigit))
        {
            ModelState.AddModelError("Input.Telefono", "El teléfono tiene que incluir números.");
            CargarVistaPrevia();
            return Page();
        }

        await settings.GuardarAsync(Input.Telefono, Input.Direccion ?? string.Empty);

        return RedirectToPage(new { guardado = true });
    }

    private void CargarVistaPrevia()
    {
        VistaPreviaWhatsApp = settings.WhatsApp;
        VistaPreviaVisible = settings.TelefonoVisible;
    }
}
