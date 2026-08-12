using AestheticCenter.Core.Entities;
using AestheticCenter.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AestheticCenter.Web.Pages.Appointments;

[Authorize]
public class MyAppointmentsModel : PageModel
{
    private readonly AppointmentRepository _appointmentRepository;
    private readonly UserManager<IdentityUser> _userManager;

    public MyAppointmentsModel(
        AppointmentRepository appointmentRepository,
        UserManager<IdentityUser> userManager)
    {
        _appointmentRepository = appointmentRepository;
        _userManager = userManager;
    }

    public List<Appointment> Appointments { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        // Sin reservas online no hay nada que listar acá.
        if (!SiteInfo.ReservaOnlineActiva)
        {
            return RedirectToPage("/Services/Index");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            Appointments = await _appointmentRepository.GetByCustomerIdAsync(user.Id);
        }

        return Page();
    }
}
