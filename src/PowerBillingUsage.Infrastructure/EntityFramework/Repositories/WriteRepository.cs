using Microsoft.EntityFrameworkCore;
using PowerBillingUsage.Domain.Abstractions.RegisteringDependencies;
using PowerBillingUsage.Domain.Abstractions.Repositories;
using PowerBillingUsage.Domain.Abstractions.Services;
using PowerBillingUsage.Domain.Abstractions;
using PowerBillingUsage.Domain.Abstractions.Helpers;

namespace PowerBillingUsage.Infrastructure.EntityFramework.Repositories;

public class WriteRepository<Entity, EntityId> : IWriteRepository<Entity, EntityId>, IScopedDependency, IDisposable
    where Entity : class, IEntity<EntityId>
    where EntityId : IEntityId
{
    protected readonly PowerBillingUsageWriteDbContext Context;
    protected readonly ICacheService CacheService;
    protected readonly ICacheKeyHelper<Entity> CacheKeyHelper;

    public WriteRepository(
        PowerBillingUsageWriteDbContext context,
        ICacheService cacheService,
        ICacheKeyHelper<Entity> cacheKeyHelper)
    {
        Context = context;
        CacheService = cacheService;
        CacheKeyHelper = cacheKeyHelper;
    }

    public async Task<Entity> InsertAsync(Entity item, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        await Task.WhenAll(
            CacheService.RemoveAsync(CacheKeyHelper.KeyAll, cancellationToken),
            CacheService.RemoveByPrefixAsync(CacheKeyHelper.PaginateKey, cancellationToken),
            UpdateCountCacheAsync(1, expiration, cancellationToken)
        );

        await Context.Set<Entity>().AddAsync(item, cancellationToken);
        string keyOne = CacheKeyHelper.MakeKeyOne(item.Id);

        await CacheService.SetAsync(keyOne, item, expiration, cancellationToken);

        return item;
    }

    public async Task DeleteAsync(EntityId id, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var item = await Context.Set<Entity>().FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        var keyOne = CacheKeyHelper.MakeKeyOne(id);

        await Task.WhenAll(
            RemoveAllCacheWithoutCountAsync(keyOne, cancellationToken),
            CacheService.RemoveByPrefixAsync(CacheKeyHelper.PaginateKey, cancellationToken),
            UpdateCountCacheAsync(-1, expiration, cancellationToken)
        );

        Context.Set<Entity>().Remove(item);
    }

    public async Task<Entity> UpdateAsync(Entity item, CancellationToken cancellationToken = default)
    {
        //_context.Entry(item).State = EntityState.Modified;
        var keyOne = CacheKeyHelper.MakeKeyOne(item.Id);

        await Task.WhenAll(
            RemoveAllCacheWithoutCountAsync(keyOne, cancellationToken),
            CacheService.RemoveByPrefixAsync(CacheKeyHelper.PaginateKey, cancellationToken)
        );

        Context.Set<Entity>().Update(item);

        return item;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await Context.SaveChangesAsync(cancellationToken);

    private Task RemoveAllCacheWithoutCountAsync(string keyOne, CancellationToken cancellationToken = default)
    {
        return Task.WhenAll(
            CacheService.RemoveAsync(CacheKeyHelper.KeyAll, cancellationToken),
            CacheService.RemoveByPrefixAsync(keyOne, cancellationToken)
        );
    }

    private async Task UpdateCountCacheAsync(int value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var count = await CacheService.GetAsync<int>(CacheKeyHelper.CountKey, cancellationToken);
        if (count is 0)
            return;

        await CacheService.RemoveAsync(CacheKeyHelper.CountKey, cancellationToken);

        await CacheService.SetAsync(CacheKeyHelper.CountKey, count + value, expiration, cancellationToken);
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
