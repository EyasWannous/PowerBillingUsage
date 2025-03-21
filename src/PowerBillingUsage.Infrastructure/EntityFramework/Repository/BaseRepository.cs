using Microsoft.EntityFrameworkCore;
using PowerBillingUsage.Domain.Abstractions;

namespace PowerBillingUsage.Infrastructure.EntityFramework.Repository;

public class BaseRepository<Entity, EntityId> : IBaseRepository<Entity, EntityId>, IDisposable
    where Entity : class, IEntity<EntityId>
    where EntityId : IEntityId
{
    private readonly PowerBillingUsageDbContext _context;

    public BaseRepository(PowerBillingUsageDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Entity>> GetlistAsync()
    {
        return await _context.Set<Entity>().ToListAsync();
    }

    public async Task<Entity?> GetByIdAsync(EntityId id)
    {
        return await _context.Set<Entity>().FindAsync(id);
    }

    public async Task InsertAsync(Entity item)
    {
        await _context.Set<Entity>().AddAsync(item);
    }

    public async Task DeleteAsync(EntityId id)
    {
        var item = await _context.Set<Entity>().FindAsync(id);
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        _context.Set<Entity>().Remove(item);
    }

    public Task UpdateAsync(Entity item)
    {
        //_context.Entry(item).State = EntityState.Modified;

        _context.Set<Entity>().Update(item);

        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    private bool disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
        }

        this.disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

}
