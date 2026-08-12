using AestheticCenter.Core.Entities;
using AestheticCenter.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AestheticCenter.Web.Pages.Services;

public class DetailModel(ServiceRepository serviceRepository) : PageModel
{
    public Service Servicio { get; private set; } = default!;

    /// <summary>
    /// Sin texto largo la página se muestra igual, para no romper la navegación
    /// desde el listado, pero se le pide a Google que no la indexe: una página
    /// con una sola frase no posiciona y resta más de lo que suma.
    /// </summary>
    public bool ListaParaGoogle => !string.IsNullOrWhiteSpace(Servicio.LongDescription);

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        // El slug sale del nombre, así que hay que traer los servicios y
        // compararlos. Ante dos nombres que den el mismo slug gana el de menor
        // Id, para que la URL siempre lleve al mismo tratamiento.
        var servicios = await serviceRepository.GetAllAsync();
        var encontrado = servicios
            .OrderBy(s => s.Id)
            .FirstOrDefault(s => Slug.Desde(s.Name) == slug);

        if (encontrado is null)
        {
            return NotFound();
        }

        Servicio = encontrado;
        return Page();
    }
}
