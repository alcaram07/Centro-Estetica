using AestheticCenter.Core.Entities;
using AestheticCenter.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AestheticCenter.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly TestimonialRepository _testimonialRepository;

    public IndexModel(ILogger<IndexModel> logger, TestimonialRepository testimonialRepository)
    {
        _logger = logger;
        _testimonialRepository = testimonialRepository;
    }

    /// <summary>Reseñas reales aprobadas por Patricia, para mostrar en la home.</summary>
    public List<Testimonial> Testimonios { get; set; } = new();

    public async Task OnGetAsync()
    {
        Testimonios = await _testimonialRepository.GetApprovedAsync(3);
    }
}
