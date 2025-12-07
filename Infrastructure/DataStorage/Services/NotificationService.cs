using microservices_project.Core.Domain;
using microservices_project.Infrastructure.DataStorage;
using Microsoft.EntityFrameworkCore;

namespace microservices_project.Infrastructure.DataStorage.Services;

public class NotificationService : IDatabaseService<Notification> // Range на всяйкий случай, но они не нужны так то
{
    private readonly ServerDbContext _context;

    public NotificationService(ServerDbContext context)
    {
        _context = context;
    }

    public async Task<Notification> AddAsync(Notification entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _context.Notifications.AddAsync(entity);
        await _context.SaveChangesAsync();

        return entity;
    }

    // public async Task<List<Notification>> AddRangeAsync(IReadOnlyList<Notification> entity)
    // {
    //     await _context.Notifications.AddRangeAsync(entity);
    //     await _context.SaveChangesAsync();

    //     return entity;
    // }

    public async Task<Notification?> FindAsync(long id) => await _context.Notifications.FirstOrDefaultAsync(m => m.Id == id);

    public async Task<List<Notification>> ListAsync() => await _context.Notifications.ToListAsync();

    public async Task<bool> RemoveAsync(Notification entity)
    {
        _context.Notifications.Remove(entity);
        await _context.SaveChangesAsync();

        return true;
    }

    // public async Task RemoveRangeAsync(IReadOnlyList<Notification> entity)
    // {
    //     _context.Notifications.RemoveRange(entity);
    //     await _context.SaveChangesAsync();
    // }
}