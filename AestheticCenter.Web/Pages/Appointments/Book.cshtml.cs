using AestheticCenter.Core.Entities;
using AestheticCenter.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AestheticCenter.Web.Pages.Appointments;

[Authorize]
public class BookModel : PageModel
{
    private readonly ServiceRepository _serviceRepository;
    private readonly AppointmentRepository _appointmentRepository;
    private readonly UserManager<IdentityUser> _userManager;

    public BookModel(
        ServiceRepository serviceRepository,
        AppointmentRepository appointmentRepository,
        UserManager<IdentityUser> userManager)
    {
        _serviceRepository = serviceRepository;
        _appointmentRepository = appointmentRepository;
        _userManager = userManager;
    }

    [BindProperty]
    public int ServiceId { get; set; }

    [BindProperty]
    public DateTime AppointmentTime { get; set; } = DateTime.Now.AddDays(1).Date.AddHours(9);

    [BindProperty]
    public string Notes { get; set; } = string.Empty;

    public Service? SelectedService { get; set; }

    public async Task<IActionResult> OnGetAsync(int serviceId)
    {
        ServiceId = serviceId;
        SelectedService = await _serviceRepository.GetByIdAsync(serviceId);

        if (SelectedService == null)
        {
            return RedirectToPage("/Services/Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        SelectedService = await _serviceRepository.GetByIdAsync(ServiceId);
        
        if (SelectedService == null)
        {
            return RedirectToPage("/Services/Index");
        }

        if (AppointmentTime < DateTime.Now)
        {
            ModelState.AddModelError(string.Empty, "La fecha de la cita no puede ser en el pasado.");
            return Page();
        }

        var isAvailable = await _appointmentRepository.IsSlotAvailableAsync(AppointmentTime, ServiceId);
        if (!isAvailable)
        {
            ModelState.AddModelError(string.Empty, "Lo sentimos, este horario ya está reservado. Por favor, elige otro.");
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var appointment = new Appointment
        {
            CustomerId = user.Id,
            ServiceId = ServiceId,
            AppointmentTime = AppointmentTime,
            Status = AppointmentStatus.Pending,
            Notes = Notes
        };

        await _appointmentRepository.AddAsync(appointment);

        return RedirectToPage("/Appointments/MyAppointments");
    }
}
