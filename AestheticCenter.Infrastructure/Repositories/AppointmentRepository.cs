using AestheticCenter.Core.Entities;
using AestheticCenter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AestheticCenter.Infrastructure.Repositories;

public class AppointmentRepository
{
    private readonly ApplicationDbContext _context;

    public AppointmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Appointment>> GetByCustomerIdAsync(string customerId)
    {
        return await _context.Appointments
            .Include(a => a.Service)
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.AppointmentTime)
            .ToListAsync();
    }

    public async Task<List<Appointment>> GetAllAsync()
    {
        return await _context.Appointments
            .Include(a => a.Service)
            .OrderByDescending(a => a.AppointmentTime)
            .ToListAsync();
    }

    public async Task UpdateStatusAsync(int id, AppointmentStatus status)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment != null)
        {
            appointment.Status = status;
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddAsync(Appointment appointment)
    {
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsSlotAvailableAsync(DateTime time, int serviceId)
    {
        // Lógica simple: no permitir dos citas exactamente a la misma hora
        // En una app real, aquí validaríamos la duración del servicio
        return !await _context.Appointments
            .AnyAsync(a => a.AppointmentTime == time && a.Status != AppointmentStatus.Cancelled);
    }
}
