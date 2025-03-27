using Microsoft.EntityFrameworkCore;
using PowerBillingUsage.Domain.Abstractions;
using System.Linq.Expressions;

namespace PowerBillingUsage.Infrastructure.EntityFramework.Repository;

public class Repository<Entity, EntityId> : IRepository<Entity, EntityId>, IScopedDependency, IDisposable
    where Entity : class, IEntity<EntityId>
    where EntityId : IEntityId
{
    protected readonly PowerBillingUsageDbContext Context;
    protected readonly ICacheService CacheService;
    protected readonly string KeyAll = $"allOf_{typeof(Entity)}";
    protected readonly string PaginateKey = $"paginate_{typeof(Entity)}_";
    protected readonly string KeyOne = $"oneOf_{typeof(Entity)}_Id: ";
    protected readonly string CountKey = $"count_{typeof(Entity)}";

    public Repository(PowerBillingUsageDbContext context, ICacheService cacheService)
    {
        Context = context;
        CacheService = cacheService;
    }

    public async Task<IEnumerable<Entity>> GetPaginateAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        var paginateKey = MakePaginateKey(skip, take);

        var items = await CacheService.GetAsync<IEnumerable<Entity>>(paginateKey, cancellationToken);
        if (items is not null)
            return items;

        items = await Context.Set<Entity>()
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync(cancellationToken: cancellationToken);

        await CacheService.SetAsync(paginateKey, items, cancellationToken);

        return items;
    }

    public async Task<IEnumerable<Entity>> GetlistAsync(CancellationToken cancellationToken = default)
    {
        var items = await CacheService.GetAsync<IEnumerable<Entity>>(KeyAll, cancellationToken);
        if (items is not null)
            return items;

        items = await Context.Set<Entity>()
            .AsNoTracking()
            .ToListAsync(cancellationToken: cancellationToken);

        await CacheService.SetAsync(KeyAll, items, cancellationToken);

        return items;
    }

    public async Task<Entity?> GetByIdAsync(EntityId id, CancellationToken cancellationToken = default)
    {
        var keyOne = MakeKeyOne(id);

        var item = await CacheService.GetAsync<Entity>(keyOne, cancellationToken);
        if (item is not null)
            return item;

        item = await Context.Set<Entity>().FirstOrDefaultAsync(x => x.Id.Value == id.Value, cancellationToken);
        if (item is null)
            return null;

        await CacheService.SetAsync(keyOne, item, cancellationToken);

        return item;
    }

    public async Task<Entity> InsertAsync(Entity item, CancellationToken cancellationToken = default)
    {
        await Task.WhenAll(
            CacheService.RemoveAsync(KeyAll, cancellationToken),
            CacheService.RemoveByPrefixAsync(PaginateKey, cancellationToken),
            UpdateCountCacheAsync(1, cancellationToken)
        );

        await Context.Set<Entity>().AddAsync(item, cancellationToken);
        string keyOne = MakeKeyOne(item.Id);

        await CacheService.SetAsync(keyOne, item, cancellationToken);

        return item;
    }

    public async Task DeleteAsync(EntityId id, CancellationToken cancellationToken = default)
    {
        var item = await Context.Set<Entity>().FirstOrDefaultAsync(x => x.Id.Value == id.Value, cancellationToken);
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        var keyOne = MakeKeyOne(id);

        await Task.WhenAll(
            RemoveAllCacheWithoutCountAsync(keyOne, cancellationToken),
            CacheService.RemoveByPrefixAsync(PaginateKey, cancellationToken),
            UpdateCountCacheAsync(-1, cancellationToken)
        );

        Context.Set<Entity>().Remove(item);
    }

    public async Task<Entity> UpdateAsync(Entity item, CancellationToken cancellationToken = default)
    {
        //_context.Entry(item).State = EntityState.Modified;
        var keyOne = MakeKeyOne(item.Id);

        await Task.WhenAll(
            RemoveAllCacheWithoutCountAsync(keyOne, cancellationToken),
            CacheService.RemoveByPrefixAsync(PaginateKey, cancellationToken)
        );

        Context.Set<Entity>().Update(item);

        return item;
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        var count = await CacheService.GetAsync<int>(CountKey, cancellationToken);
        if (count is not 0)
            return count;

        count = await Context.Set<Entity>().CountAsync(cancellationToken);

        await CacheService.SetAsync(CountKey, count, cancellationToken);

        return count;
    }

    public async Task<int> CountAsync(Expression<Func<Entity, bool>> criteria, CancellationToken cancellationToken = default)
        => await Context.Set<Entity>().CountAsync(criteria, cancellationToken);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) 
        => await Context.SaveChangesAsync(cancellationToken);

    private string MakeKeyOne(EntityId id) => KeyOne + id.Value.ToString();

    private string MakePaginateKey(int skip, int take) 
        => PaginateKey + "skip: " + skip + "_take: " + take;

    private async Task RemoveAllCacheWithoutCountAsync(string keyOne, CancellationToken cancellationToken = default)
    {
        await Task.WhenAll(
            CacheService.RemoveAsync(KeyAll, cancellationToken),
            CacheService.RemoveByPrefixAsync(keyOne, cancellationToken)
        );
    }

    private async Task UpdateCountCacheAsync(int value, CancellationToken cancellationToken)
    {
        var count = await CacheService.GetAsync<int>(CountKey, cancellationToken);
        if (count is 0)
            return;

        await CacheService.RemoveAsync(CountKey, cancellationToken);
        
        await CacheService.SetAsync(CountKey, count + value, cancellationToken);
    }

    private bool disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            if (disposing)
            {
                Context.Dispose();
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
