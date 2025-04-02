using Microsoft.EntityFrameworkCore;
using PowerBillingUsage.Domain.Abstractions;
using PowerBillingUsage.Domain.Abstractions.Helpers;
using PowerBillingUsage.Domain.Abstractions.RegisteringDependencies;
using PowerBillingUsage.Domain.Abstractions.Repositories;
using PowerBillingUsage.Domain.Abstractions.Services;
using PowerBillingUsage.Domain.Abstractions.Shared;
using System.Linq.Expressions;

namespace PowerBillingUsage.Infrastructure.EntityFramework.Repositories;

public class Repository<Entity, EntityId> : IRepository<Entity, EntityId>, IScopedDependency, IDisposable
    where Entity : class, IEntity<EntityId>
    where EntityId : IEntityId
{
    protected readonly PowerBillingUsageWriteDbContext Context;
    protected readonly ICacheService CacheService;
    protected readonly ICacheKeyHelper<Entity> CacheKeyHelper;

    public Repository(
        PowerBillingUsageWriteDbContext context,
        ICacheService cacheService,
        ICacheKeyHelper<Entity> cacheKeyHelper)
    {
        Context = context;
        CacheService = cacheService;
        CacheKeyHelper = cacheKeyHelper;
    }

    public async Task<PaingationResponse<Entity>> GetPaginateAsync(int skip, int take, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var paginateKey = CacheKeyHelper.MakePaginateKey(skip, take);

        var cachedResult = await CacheService.GetAsync<PaingationResponse<Entity>>(paginateKey, cancellationToken);
        if (cachedResult is not null)
            return cachedResult;

        var items = await Context.Set<Entity>()
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalCount = await Context.Set<Entity>().CountAsync(cancellationToken);
        var result = new PaingationResponse<Entity>(totalCount, items);

        await CacheService.SetAsync(paginateKey, result, expiration, cancellationToken);

        return result;
    }

    public async Task<IEnumerable<Entity>> GetlistAsync(TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var items = await CacheService.GetAsync<IEnumerable<Entity>>(CacheKeyHelper.KeyAll, cancellationToken);
        if (items is not null)
            return items;

        items = await Context.Set<Entity>()
            .AsNoTracking()
            .ToListAsync(cancellationToken: cancellationToken);

        await CacheService.SetAsync(CacheKeyHelper.KeyAll, items, expiration, cancellationToken);

        return items;
    }

    public async Task<Entity?> GetByIdAsync(EntityId id, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var keyOne = CacheKeyHelper.MakeKeyOne(id);

        var item = await CacheService.GetAsync<Entity>(keyOne, cancellationToken);
        if (item is not null)
            return item;

        item = await Context.Set<Entity>().FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);
        if (item is null)
            return null;

        await CacheService.SetAsync(keyOne, item, expiration, cancellationToken);

        return item;
    }

    public Task<IQueryable<Entity>> GetQueryableAsync()
    {
        return Task.FromResult(Context.Set<Entity>().AsQueryable());
    }

    public async Task<IQueryable<Entity>> GetQueryableWithDetailsAsync(params List<Expression<Func<Entity, object>>>? navigationPropertyPaths)
    {
        var query = await GetQueryableAsync();
        if (navigationPropertyPaths is not null)
        {
            foreach (var navigationPropertyPath in navigationPropertyPaths)
                query = query.Include(navigationPropertyPath);
        }

        return query;
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

    public async Task<int> CountAsync(TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var count = await CacheService.GetAsync<int>(CacheKeyHelper.CountKey, cancellationToken);
        if (count is not 0)
            return count;

        count = await Context.Set<Entity>().CountAsync(cancellationToken);

        await CacheService.SetAsync(CacheKeyHelper.CountKey, count, expiration, cancellationToken);

        return count;
    }

    public async Task<int> CountAsync(Expression<Func<Entity, bool>> criteria, CancellationToken cancellationToken = default)
        => await Context.Set<Entity>().CountAsync(criteria, cancellationToken);

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
