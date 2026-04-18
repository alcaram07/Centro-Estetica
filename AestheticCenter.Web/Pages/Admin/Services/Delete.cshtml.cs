using AestheticCenter.Core.Entities;
using AestheticCenter.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AestheticCenter.Web.Pages.Admin.Services;

[Authorize(Roles = "Admin")]
public class DeleteModel : PageModel
{
    private readonly ServiceRepository _serviceRepository;

    public DeleteModel(ServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    [BindProperty]
    public Service Service { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var service = await _serviceRepository.GetByIdAsync(id);
        if (service == null)
        {
            return NotFound();
        }
        Service = service;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        await _serviceRepository.DeleteAsync(id);
        return RedirectToPage("./Index");
    }
}
