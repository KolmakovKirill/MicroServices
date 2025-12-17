namespace CommonShared.Infrastructure.DataStorage.Services;

public interface IDatabaseService<T>
{
    Task<T> AddAsync (T entity);
    // Task<List<T>> AddRangeAsync (IReadOnlyList<T> entity);
    Task<T?> FindAsync (long id);
    Task<List<T>> ListAsync();
    Task<bool> RemoveAsync (T entity);
    Task<T?> UpdateAsync (T entity); 
    // Task RemoveRangeAsync (IReadOnlyList<T> entity);
}