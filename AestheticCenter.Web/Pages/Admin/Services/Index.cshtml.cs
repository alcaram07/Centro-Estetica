using AestheticCenter.Core.Entities;
using AestheticCenter.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AestheticCenter.Web.Pages.Admin.Services;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ServiceRepository _serviceRepository;

    public IndexModel(ServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public List<Service> Services { get; set; } = new();

    public async Task OnGetAsync()
    {
        Services = await _serviceRepository.GetAllAsync();
    }
}
