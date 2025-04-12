using Microsoft.EntityFrameworkCore;
using PowerBillingUsage.Domain.Abstractions;
using PowerBillingUsage.Domain.Abstractions.Helpers;
using PowerBillingUsage.Domain.Abstractions.RegisteringDependencies;
using PowerBillingUsage.Domain.Abstractions.Repositories;
using PowerBillingUsage.Domain.Abstractions.Services;

namespace PowerBillingUsage.Infrastructure.EntityFramework.Repositories;

public class WriteRepository<Entity, EntityId> :
    BaseRepository<Entity, EntityId, PowerBillingUsageWriteDbContext>,
    IWriteRepository<Entity, EntityId>,
    IScopedDependency
    where Entity : class, IEntity<EntityId>
    where EntityId : IEntityId
{
    public WriteRepository(
        PowerBillingUsageWriteDbContext context,
        IHybridCacheService cacheService,
        ICacheKeyHelper<Entity> cacheKeyHelper)
        : base(context, cacheService, cacheKeyHelper)
    {
    }

    public async Task<Entity> InsertAsync(Entity item, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        await Task.WhenAll(
            CacheService.RemoveAsync(CacheKeyHelper.KeyAll, cancellationToken).AsTask(),
            CacheService.RemoveByTagsAsync([CacheKeyHelper.PaginateKey], cancellationToken).AsTask(),
            UpdateCountCacheAsync(1, expiration, cancellationToken)
        );

        await Context.Set<Entity>().AddAsync(item, cancellationToken);
        string keyOne = CacheKeyHelper.MakeKeyOne(item.Id);

        await CacheService.SetAsync(
            keyOne,
            item,
            tags: [keyOne],
            expiration: expiration,
            cancellationToken: cancellationToken
        );

        return item;
    }

    public async Task DeleteAsync(EntityId id, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var item = await Context.Set<Entity>().FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        var keyOne = CacheKeyHelper.MakeKeyOne(id);

        await Task.WhenAll(
            CacheService.RemoveAsync(CacheKeyHelper.KeyAll, cancellationToken).AsTask(),
            CacheService.RemoveAsync(keyOne, cancellationToken).AsTask(),
            CacheService.RemoveByTagsAsync([CacheKeyHelper.PaginateKey], cancellationToken).AsTask(),
            UpdateCountCacheAsync(-1, expiration, cancellationToken)
        );

        Context.Set<Entity>().Remove(item);
    }

    public async Task DeleteRangeAsync(List<EntityId> itemsIds, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var items = await Context.Set<Entity>().Where(x => itemsIds.Contains(x.Id)).ToListAsync();
        if (items.Count != itemsIds.Count)
            throw new ArgumentNullException(nameof(items));

        var tasks = new List<Task>();
        foreach (var itemId in itemsIds)
        {
            var keyOne = CacheKeyHelper.MakeKeyOne(itemId);
            tasks.Add(CacheService.RemoveAsync(keyOne, cancellationToken).AsTask());
        }

        tasks.AddRange(
            CacheService.RemoveAsync(CacheKeyHelper.KeyAll, cancellationToken).AsTask(),
            CacheService.RemoveByTagsAsync([CacheKeyHelper.PaginateKey], cancellationToken).AsTask(),
            UpdateCountCacheAsync(-1, expiration, cancellationToken)
        );

        await Task.WhenAll(tasks);

        Context.Set<Entity>().RemoveRange(items);
    }

    public async Task<int> DeleteAndSaveChangesAsync(Entity item, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync(cancellationToken);

        var deletedItemsNumber = await Context.Set<Entity>().Where(x => x.Id.Equals(item.Id)).ExecuteDeleteAsync(cancellationToken);
        if (deletedItemsNumber is 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new ArgumentNullException(nameof(deletedItemsNumber));
        }

        var keyOne = CacheKeyHelper.MakeKeyOne(item.Id);

        await Task.WhenAll(
            CacheService.RemoveAsync(CacheKeyHelper.KeyAll, cancellationToken).AsTask(),
            CacheService.RemoveAsync(keyOne, cancellationToken).AsTask(),
            CacheService.RemoveByTagsAsync([CacheKeyHelper.PaginateKey], cancellationToken).AsTask(),
            UpdateCountCacheAsync(-1, expiration, cancellationToken)
        );

        await transaction.CommitAsync(cancellationToken);

        return deletedItemsNumber;
    }

    public async Task<int> DeleteRangeAndSaveChangesAsync(List<Entity> items, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync(cancellationToken);

        var itemsIds = items.Select(x => x.Id).ToList();

        var deletedItemsNumber = await Context.Set<Entity>().Where(x => itemsIds.Contains(x.Id)).ExecuteDeleteAsync(cancellationToken);
        if (deletedItemsNumber is 0 || deletedItemsNumber != itemsIds.Count)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new ArgumentNullException(nameof(deletedItemsNumber));
        }

        var tasks = new List<Task>();
        foreach (var itemId in itemsIds)
        {
            var keyOne = CacheKeyHelper.MakeKeyOne(itemId);
            tasks.Add(CacheService.RemoveAsync(keyOne, cancellationToken).AsTask());
        }

        tasks.AddRange(
            CacheService.RemoveAsync(CacheKeyHelper.KeyAll, cancellationToken).AsTask(),
            CacheService.RemoveByTagsAsync([CacheKeyHelper.PaginateKey], cancellationToken).AsTask(),
            UpdateCountCacheAsync(-items.Count, expiration, cancellationToken)
        );

        await Task.WhenAll(tasks);

        await transaction.CommitAsync(cancellationToken);

        return deletedItemsNumber;
    }

    public async Task<Entity> UpdateAsync(Entity item, CancellationToken cancellationToken = default)
    {
        var keyOne = CacheKeyHelper.MakeKeyOne(item.Id);

        await Task.WhenAll(
            CacheService.RemoveAsync(CacheKeyHelper.KeyAll, cancellationToken).AsTask(),
            CacheService.RemoveAsync(keyOne, cancellationToken).AsTask(),
            CacheService.RemoveByTagsAsync([CacheKeyHelper.PaginateKey], cancellationToken).AsTask()
        );

        Context.Set<Entity>().Update(item);

        return item;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await Context.SaveChangesAsync(cancellationToken);

    private async Task UpdateCountCacheAsync(int value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var count = await CacheService.GetOrCreateAsync<int>(
            CacheKeyHelper.CountKey,
            async ct => await Context.Set<Entity>().CountAsync(ct),
            tags: [CacheKeyHelper.CountKey],
            expiration: expiration,
            cancellationToken: cancellationToken
        );

        await CacheService.RemoveAsync(CacheKeyHelper.CountKey, cancellationToken);

        await CacheService.SetAsync(
            CacheKeyHelper.CountKey,
            count + value,
            tags: [CacheKeyHelper.CountKey],
            expiration: expiration,
            cancellationToken: cancellationToken
        );
    }
}