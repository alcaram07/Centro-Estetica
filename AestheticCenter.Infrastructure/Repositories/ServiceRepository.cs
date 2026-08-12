using AestheticCenter.Core.Entities;
using AestheticCenter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AestheticCenter.Infrastructure.Repositories;

public class ServiceRepository
{
    private readonly ApplicationDbContext _context;

    public ServiceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Service>> GetAllAsync()
    {
        return await _context.Services.ToListAsync();
    }

    public async Task<Service?> GetByIdAsync(int id)
    {
        return await _context.Services.FindAsync(id);
    }

    // La fecha se marca acá y no en las páginas del panel para que valga sea cual
    // sea el camino que termine guardando un servicio. Es la que el sitemap le
    // declara a Google como lastmod.

    public async Task AddAsync(Service service)
    {
        service.UpdatedAt = DateTime.UtcNow;
        _context.Services.Add(service);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Service service)
    {
        service.UpdatedAt = DateTime.UtcNow;
        _context.Entry(service).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service != null)
        {
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
        }
    }
}
