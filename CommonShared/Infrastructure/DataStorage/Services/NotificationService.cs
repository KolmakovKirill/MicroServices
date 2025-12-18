using CommonShared.Core.Domain;
using CommonShared.Infrastructure.DataStorage;
using Microsoft.EntityFrameworkCore;

namespace CommonShared.Infrastructure.DataStorage.Services;

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

    public async Task<Notification?> FindAsync(long id) => await _context.Notifications
        .Include(n => n.User)
        .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<List<Notification>> ListAsync() => await _context.Notifications.ToListAsync();

    public async Task<bool> RemoveAsync(Notification entity)
    {
        _context.Notifications.Remove(entity);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<Notification?> UpdateAsync(Notification entity)
    {
        var found = await this.FindAsync(entity.Id);
        if (found == null)
            return null;
        _context.Notifications.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    // public async Task RemoveRangeAsync(IReadOnlyList<Notification> entity)
    // {
    //     _context.Notifications.RemoveRange(entity);
    //     await _context.SaveChangesAsync();
    // }
}