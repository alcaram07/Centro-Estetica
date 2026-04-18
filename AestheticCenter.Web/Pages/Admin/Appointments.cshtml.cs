using AestheticCenter.Core.Entities;
using AestheticCenter.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AestheticCenter.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class AppointmentsModel : PageModel
{
    private readonly AppointmentRepository _appointmentRepository;

    public AppointmentsModel(AppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public List<Appointment> Appointments { get; set; } = new();

    public async Task OnGetAsync()
    {
        Appointments = await _appointmentRepository.GetAllAsync();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, AppointmentStatus status)
    {
        await _appointmentRepository.UpdateStatusAsync(id, status);
        return RedirectToPage();
    }
}
