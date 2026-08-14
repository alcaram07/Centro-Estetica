using AestheticCenter.Core.Entities;
using AestheticCenter.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AestheticCenter.Web.Pages.Resenas;

public class IndexModel : PageModel
{
    private readonly TestimonialRepository _testimonialRepository;

    public IndexModel(TestimonialRepository testimonialRepository)
    {
        _testimonialRepository = testimonialRepository;
    }

    [BindProperty]
    public Testimonial Resena { get; set; } = default!;

    /// <summary>
    /// Campo trampa para bots: invisible para una persona, pero un formulario
    /// automático que completa todos los campos lo llena. Si llega con algo
    /// adentro, se descarta la reseña sin decírselo ni devolver un error.
    /// </summary>
    [BindProperty]
    public string? Web { get; set; }

    public bool Enviada { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!string.IsNullOrEmpty(Web))
        {
            Enviada = true;
            return Page();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _testimonialRepository.AddAsync(Resena);
        Enviada = true;
        ModelState.Clear();
        Resena = new Testimonial();
        return Page();
    }
}
