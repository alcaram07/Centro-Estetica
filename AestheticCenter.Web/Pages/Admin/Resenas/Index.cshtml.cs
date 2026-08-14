using AestheticCenter.Core.Entities;
using AestheticCenter.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AestheticCenter.Web.Pages.Admin.Resenas;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly TestimonialRepository _testimonialRepository;

    public IndexModel(TestimonialRepository testimonialRepository)
    {
        _testimonialRepository = testimonialRepository;
    }

    public List<Testimonial> Testimonials { get; set; } = new();

    public async Task OnGetAsync()
    {
        Testimonials = await _testimonialRepository.GetAllAsync();
    }

    public async Task<IActionResult> OnPostAprobarAsync(int id)
    {
        await _testimonialRepository.SetApprovedAsync(id, true);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostOcultarAsync(int id)
    {
        await _testimonialRepository.SetApprovedAsync(id, false);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        await _testimonialRepository.DeleteAsync(id);
        return RedirectToPage();
    }
}
