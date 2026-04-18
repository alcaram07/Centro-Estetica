using AestheticCenter.Core.Entities;
using AestheticCenter.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AestheticCenter.Web.Pages.Admin.Services;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly ServiceRepository _serviceRepository;

    public CreateModel(ServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    [BindProperty]
    public Service Service { get; set; } = default!;

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _serviceRepository.AddAsync(Service);

        return RedirectToPage("./Index");
    }
}
