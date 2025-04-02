using Microsoft.EntityFrameworkCore;
using PowerBillingUsage.Domain.Abstractions;
using PowerBillingUsage.Domain.Abstractions.Helpers;
using PowerBillingUsage.Domain.Abstractions.RegisteringDependencies;
using PowerBillingUsage.Domain.Abstractions.Repositories;
using PowerBillingUsage.Domain.Abstractions.Services;
using PowerBillingUsage.Domain.Abstractions.Shared;
using System.Linq.Expressions;

namespace PowerBillingUsage.Infrastructure.EntityFramework.Repositories;

public class ReadRepository<ReadModel, EntityId> : IReadRepository<ReadModel, EntityId>, IScopedDependency, IDisposable
    where ReadModel : class, IReadModel<EntityId>
    where EntityId : IEntityId
{
    protected readonly PowerBillingUsageReadDbContext Context;
    protected readonly ICacheService CacheService;
    protected readonly ICacheKeyHelper<ReadModel> CacheKeyHelper;

    public ReadRepository(
        PowerBillingUsageReadDbContext context,
        ICacheService cacheService,
        ICacheKeyHelper<ReadModel> cacheKeyHelper)
    {
        Context = context;
        CacheService = cacheService;
        CacheKeyHelper = cacheKeyHelper;
    }
    
    public async Task<PaingationResponse<ReadModel>> GetPaginateAsync(int skip, int take, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var paginateKey = CacheKeyHelper.MakePaginateKey(skip, take);

        var cachedResult = await CacheService.GetAsync<PaingationResponse<ReadModel>>(paginateKey, cancellationToken);
        if (cachedResult is not null)
            return cachedResult;

        var items = await Context.Set<ReadModel>()
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalCount = await Context.Set<ReadModel>().CountAsync(cancellationToken);
        var result = new PaingationResponse<ReadModel>(totalCount, items);

        await CacheService.SetAsync(paginateKey, result, expiration, cancellationToken);

        return result;
    }

    public async Task<IEnumerable<ReadModel>> GetlistAsync(TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var items = await CacheService.GetAsync<IEnumerable<ReadModel>>(CacheKeyHelper.KeyAll, cancellationToken);
        if (items is not null)
            return items;

        items = await Context.Set<ReadModel>()
            .AsNoTracking()
            .ToListAsync(cancellationToken: cancellationToken);

        await CacheService.SetAsync(CacheKeyHelper.KeyAll, items, expiration, cancellationToken);

        return items;
    }

    public async Task<ReadModel?> GetByIdAsync(EntityId id, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var keyOne = CacheKeyHelper.MakeKeyOne(id);

        var item = await CacheService.GetAsync<ReadModel>(keyOne, cancellationToken);
        if (item is not null)
            return item;

        item = await Context.Set<ReadModel>().FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);
        if (item is null)
            return null;

        await CacheService.SetAsync(keyOne, item, expiration, cancellationToken);

        return item;
    }

    public Task<IQueryable<ReadModel>> GetQueryableAsync()
    {
        return Task.FromResult(Context.Set<ReadModel>().AsQueryable());
    }

    public async Task<IQueryable<ReadModel>> GetQueryableWithDetailsAsync(params List<Expression<Func<ReadModel, object>>>? navigationPropertyPaths)
    {
        var query = await GetQueryableAsync();
        if (navigationPropertyPaths is not null)
        {
            foreach (var navigationPropertyPath in navigationPropertyPaths)
                query = query.Include(navigationPropertyPath);
        }

        return query;
    }

    public async Task<int> CountAsync(TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var count = await CacheService.GetAsync<int>(CacheKeyHelper.CountKey, cancellationToken);
        if (count is not 0)
            return count;

        count = await Context.Set<ReadModel>().CountAsync(cancellationToken);

        await CacheService.SetAsync(CacheKeyHelper.CountKey, count, expiration, cancellationToken);

        return count;
    }

    public async Task<int> CountAsync(Expression<Func<ReadModel, bool>> criteria, CancellationToken cancellationToken = default)
        => await Context.Set<ReadModel>().CountAsync(criteria, cancellationToken);

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
