using Microsoft.EntityFrameworkCore;
using PowerBillingUsage.Domain.Abstractions;
using PowerBillingUsage.Domain.Abstractions.Helpers;
using PowerBillingUsage.Domain.Abstractions.RegisteringDependencies;
using PowerBillingUsage.Domain.Abstractions.Repositories;
using PowerBillingUsage.Domain.Abstractions.Services;
using System.Linq.Expressions;

namespace PowerBillingUsage.Infrastructure.EntityFramework.Repositories;

public class Repository<Entity, EntityId> : IRepository<Entity, EntityId>, IScopedDependency, IDisposable
    where Entity : class, IEntity<EntityId>
    where EntityId : IEntityId
{
    protected readonly PowerBillingUsageWriteDbContext Context;
    protected readonly ICacheService CacheService;
    protected readonly ICacheInvalidationHelper CacheInvalidationHelper;

    protected readonly string KeyAll = $"allOf_{typeof(Entity)}";
    protected readonly string PaginateKey = $"paginate_{typeof(Entity)}_";
    protected readonly string KeyOne = $"oneOf_{typeof(Entity)}_Id: ";
    protected readonly string CountKey = $"count_{typeof(Entity)}";

    public Repository(
        PowerBillingUsageWriteDbContext context,
        ICacheService cacheService,
        ICacheInvalidationHelper cacheInvalidationHelper)
    {
        Context = context;
        CacheService = cacheService;
        CacheInvalidationHelper = cacheInvalidationHelper;
    }

    public async Task<IEnumerable<Entity>> GetPaginateAsync(int skip, int take, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
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

        await CacheService.SetAsync(paginateKey, items, expiration, cancellationToken);

        return items;
    }

    public async Task<IEnumerable<Entity>> GetlistAsync(TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var items = await CacheService.GetAsync<IEnumerable<Entity>>(KeyAll, cancellationToken);
        if (items is not null)
            return items;

        items = await Context.Set<Entity>()
            .AsNoTracking()
            .ToListAsync(cancellationToken: cancellationToken);

        await CacheService.SetAsync(KeyAll, items, expiration, cancellationToken);

        return items;
    }

    public async Task<Entity?> GetByIdAsync(EntityId id, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var keyOne = MakeKeyOne(id);

        var item = await CacheService.GetAsync<Entity>(keyOne, cancellationToken);
        if (item is not null)
            return item;

        item = await Context.Set<Entity>().FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);
        if (item is null)
            return null;

        await CacheService.SetAsync(keyOne, item, expiration, cancellationToken);

        return item;
    }

    public async Task<Entity> InsertAsync(Entity item, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var tasks = new List<Task>();
        tasks.AddRange(
            CacheService.RemoveAsync(KeyAll, cancellationToken),
            CacheService.RemoveByPrefixAsync(PaginateKey, cancellationToken),
            UpdateCountCacheAsync(1, expiration, cancellationToken)
        );

        // Invalidate related ReadModel caches
        var relatedTypes = CacheInvalidationHelper.GetRelatedTypes(typeof(Entity));
        foreach (var readModelType in relatedTypes)
        {
            var readModelKeyAll = $"allOf_{readModelType}";
            var readModelPaginateKey = $"paginate_{readModelType}_";
            var readModelCountKey = $"count_{readModelType}";
            tasks.AddRange(
                CacheService.RemoveAsync(readModelKeyAll, cancellationToken),
                CacheService.RemoveByPrefixAsync(readModelPaginateKey, cancellationToken),
                UpdateRelatedCountCacheAsync(readModelCountKey, 1, expiration, cancellationToken)
            );
        }

        await Task.WhenAll(tasks);

        await Context.Set<Entity>().AddAsync(item, cancellationToken);
        string keyOne = MakeKeyOne(item.Id);

        await CacheService.SetAsync(keyOne, item, expiration, cancellationToken);

        return item;
    }

    public async Task DeleteAsync(EntityId id, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var item = await Context.Set<Entity>().FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        var keyOne = MakeKeyOne(id);

        var tasks = new List<Task>();
        tasks.AddRange(
            RemoveAllCacheWithoutCountAsync(keyOne, cancellationToken),
            CacheService.RemoveByPrefixAsync(PaginateKey, cancellationToken),
            UpdateCountCacheAsync(-1, expiration, cancellationToken)
        );

        // Invalidate related ReadModel caches
        var relatedTypes = CacheInvalidationHelper.GetRelatedTypes(typeof(Entity));
        foreach (var readModelType in relatedTypes)
        {
            var readModelKeyAll = $"allOf_{readModelType}";
            var readModelPaginateKey = $"paginate_{readModelType}_";
            var readModelKeyOne = $"oneOf_{readModelType}_Id: {id.Value}";
            var readModelCountKey = $"count_{readModelType}";
            tasks.AddRange(
                CacheService.RemoveAsync(readModelKeyAll, cancellationToken),
                CacheService.RemoveAsync(readModelKeyOne, cancellationToken),
                CacheService.RemoveByPrefixAsync(readModelPaginateKey, cancellationToken),
                UpdateRelatedCountCacheAsync(readModelCountKey, -1, expiration, cancellationToken)
            );
        }

        await Task.WhenAll(tasks);

        Context.Set<Entity>().Remove(item);
    }

    public async Task<Entity> UpdateAsync(Entity item, CancellationToken cancellationToken = default)
    {
        //_context.Entry(item).State = EntityState.Modified;
        var keyOne = MakeKeyOne(item.Id);

        var tasks = new List<Task>();
        tasks.AddRange(
            RemoveAllCacheWithoutCountAsync(keyOne, cancellationToken),
            CacheService.RemoveByPrefixAsync(PaginateKey, cancellationToken)
        );

        // Invalidate related ReadModel caches (no count update needed for update)
        var relatedTypes = CacheInvalidationHelper.GetRelatedTypes(typeof(Entity));
        foreach (var readModelType in relatedTypes)
        {
            var readModelKeyAll = $"allOf_{readModelType}";
            var readModelPaginateKey = $"paginate_{readModelType}_";
            var readModelKeyOne = $"oneOf_{readModelType}_Id: {item.Id.Value}";
            tasks.AddRange(
                CacheService.RemoveAsync(readModelKeyAll, cancellationToken),
                CacheService.RemoveAsync(readModelKeyOne, cancellationToken),
                CacheService.RemoveByPrefixAsync(readModelPaginateKey, cancellationToken)
            );
        }

        await Task.WhenAll(tasks);

        Context.Set<Entity>().Update(item);

        return item;
    }

    public async Task<int> CountAsync(TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var count = await CacheService.GetAsync<int>(CountKey, cancellationToken);
        if (count is not 0)
            return count;

        count = await Context.Set<Entity>().CountAsync(cancellationToken);

        await CacheService.SetAsync(CountKey, count, expiration, cancellationToken);

        return count;
    }

    public async Task<int> CountAsync(Expression<Func<Entity, bool>> criteria, CancellationToken cancellationToken = default)
        => await Context.Set<Entity>().CountAsync(criteria, cancellationToken);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await Context.SaveChangesAsync(cancellationToken);

    private string MakeKeyOne(EntityId id) => KeyOne + id.Value.ToString();

    private string MakePaginateKey(int skip, int take)
        => PaginateKey + "skip: " + skip + "_take: " + take;

    private Task RemoveAllCacheWithoutCountAsync(string keyOne, CancellationToken cancellationToken = default)
    {
        return Task.WhenAll(
            CacheService.RemoveAsync(KeyAll, cancellationToken),
            CacheService.RemoveByPrefixAsync(keyOne, cancellationToken)
        );
    }

    private async Task UpdateCountCacheAsync(int value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var count = await CacheService.GetAsync<int>(CountKey, cancellationToken);
        if (count is 0)
            return;

        await CacheService.RemoveAsync(CountKey, cancellationToken);

        await CacheService.SetAsync(CountKey, count + value, expiration, cancellationToken);
    }

    private async Task UpdateRelatedCountCacheAsync(string countKey, int value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var count = await CacheService.GetAsync<int>(countKey, cancellationToken);
        if (count is 0)
            return;

        await CacheService.RemoveAsync(countKey, cancellationToken);
        await CacheService.SetAsync(countKey, count + value, expiration, cancellationToken);
    }

    private bool disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                Context.Dispose();
            }
        }

        disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

}
