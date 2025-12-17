using CommonShared.Core.Domain;
using CommonShared.Infrastructure.DataStorage;
using Microsoft.EntityFrameworkCore;

namespace CommonShared.Infrastructure.DataStorage.Services;

public class UserService : IDatabaseService<User> // Range на всяйкий случай, но они не нужны так то
{
    private readonly ServerDbContext _context;

    public UserService(ServerDbContext context)
    {
        _context = context;
    }

    public async Task<User> AddAsync(User entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _context.Users.AddAsync(entity);
        await _context.SaveChangesAsync();

        return entity;
    }

    // public async Task<List<User>> AddRangeAsync(IReadOnlyList<User> entity)
    // {
    //     await _context.Users.AddRangeAsync(entity);
    //     await _context.SaveChangesAsync();

    //     return entity;
    // }

    public async Task<User?> FindAsync(long id) => await _context.Users.FirstOrDefaultAsync(m => m.Id == id);

    public async Task<List<User>> ListAsync() => await _context.Users.ToListAsync();

    public async Task<bool> RemoveAsync(User entity)
    {
        _context.Users.Remove(entity);
        await _context.SaveChangesAsync();

        return true;
    }

    // public async Task RemoveRangeAsync(IReadOnlyList<User> entity)
    // {
    //     _context.Users.RemoveRange(entity);
    //     await _context.SaveChangesAsync();
    // }

    public async Task<User?> UpdateAsync(User entity)
    {
        var found = await this.FindAsync(entity.Id);
        if (found == null)
            return null;
        _context.Users.Update(entity);
        _context.SaveChangesAsync();
        return entity;
    }
}