using AestheticCenter.Core.Entities;
using AestheticCenter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AestheticCenter.Infrastructure.Repositories;

public class TestimonialRepository
{
    private readonly ApplicationDbContext _context;

    public TestimonialRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Para el panel: todas, las más nuevas primero.</summary>
    public async Task<List<Testimonial>> GetAllAsync()
    {
        return await _context.Testimonials
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    /// <summary>Para la home: solo las que Patricia aprobó, las más nuevas primero.</summary>
    public async Task<List<Testimonial>> GetApprovedAsync(int max)
    {
        return await _context.Testimonials
            .Where(t => t.Approved)
            .OrderByDescending(t => t.CreatedAt)
            .Take(max)
            .ToListAsync();
    }

    public async Task AddAsync(Testimonial testimonial)
    {
        testimonial.CreatedAt = DateTime.UtcNow;
        testimonial.Approved = false;
        _context.Testimonials.Add(testimonial);
        await _context.SaveChangesAsync();
    }

    public async Task SetApprovedAsync(int id, bool aprobado)
    {
        var testimonial = await _context.Testimonials.FindAsync(id);
        if (testimonial != null)
        {
            testimonial.Approved = aprobado;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var testimonial = await _context.Testimonials.FindAsync(id);
        if (testimonial != null)
        {
            _context.Testimonials.Remove(testimonial);
            await _context.SaveChangesAsync();
        }
    }
}
