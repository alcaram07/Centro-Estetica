using AestheticCenter.Core.Entities;
using AestheticCenter.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AestheticCenter.Web.Pages.Admin.Services;

[Authorize(Roles = "Admin")]
public class EditModel : PageModel
{
    private readonly ServiceRepository _serviceRepository;

    public EditModel(ServiceRepository serviceRepository)
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

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _serviceRepository.UpdateAsync(Service);
        return RedirectToPage("./Index");
    }
}
