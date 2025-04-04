using Microsoft.EntityFrameworkCore;
using PowerBillingUsage.Domain.Abstractions;
using PowerBillingUsage.Domain.Abstractions.Helpers;
using PowerBillingUsage.Domain.Abstractions.Repositories;
using PowerBillingUsage.Domain.Abstractions.Services;
using PowerBillingUsage.Domain.Abstractions.Shared;
using System.Linq.Expressions;

namespace PowerBillingUsage.Infrastructure.EntityFramework.Repositories;

public abstract class BaseRepository<TModel, TEntityId, TDbContext> : IBaseRepository<TModel, TEntityId>, IDisposable
    where TModel : class, IHasId<TEntityId>
    where TEntityId : IEntityId
    where TDbContext : DbContext
{
    protected readonly TDbContext Context;
    protected readonly IHybridCacheService CacheService;
    protected readonly ICacheKeyHelper<TModel> CacheKeyHelper;

    protected BaseRepository(
        TDbContext context,
        IHybridCacheService cacheService,
        ICacheKeyHelper<TModel> cacheKeyHelper)
    {
        Context = context;
        CacheService = cacheService;
        CacheKeyHelper = cacheKeyHelper;
    }

    public async Task<PaingationResponse<TModel>> GetPaginateAsync(int skip, int take, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var paginateKey = CacheKeyHelper.MakePaginateKey(skip, take);

        return await CacheService.GetOrCreateAsync(
            paginateKey,
            async ct =>
            {
                var items = await Context.Set<TModel>()
                    .Skip(skip)
                    .Take(take)
                    .AsNoTracking()
                    .ToListAsync(ct);

                var totalCount = await Context.Set<TModel>().CountAsync(ct);
                return new PaingationResponse<TModel>(totalCount, items);
            },
            tags: [CacheKeyHelper.PaginateKey],
            expiration: expiration,
            cancellationToken: cancellationToken
        );
    }

    public async Task<IEnumerable<TModel>> GetListAsync(TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        return await CacheService.GetOrCreateAsync<IEnumerable<TModel>>(
            CacheKeyHelper.KeyAll,
            async ct => await Context.Set<TModel>()
                .AsNoTracking()
                .ToListAsync(cancellationToken: ct),
            tags: [CacheKeyHelper.KeyAll],
            expiration: expiration,
            cancellationToken: cancellationToken
        );
    }

    public async Task<TModel?> GetByIdAsync(TEntityId id, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var keyOne = CacheKeyHelper.MakeKeyOne(id);

        return await CacheService.GetOrCreateAsync(
            keyOne,
            async ct => await Context.Set<TModel>().FirstOrDefaultAsync(x => x.Id.Equals(id), ct),
            tags: [keyOne],
            expiration: expiration,
            cancellationToken: cancellationToken
        );
    }

    public Task<IQueryable<TModel>> GetQueryableAsync()
    {
        return Task.FromResult(Context.Set<TModel>().AsQueryable());
    }

    public async Task<IQueryable<TModel>> GetQueryableWithDetailsAsync(params List<Expression<Func<TModel, object>>>? navigationPropertyPaths)
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
        return await CacheService.GetOrCreateAsync<int>(
            CacheKeyHelper.CountKey,
            async ct => await Context.Set<TModel>().CountAsync(ct),
            tags: new[] { CacheKeyHelper.CountKey },
            expiration: expiration,
            cancellationToken: cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<TModel, bool>> criteria, CancellationToken cancellationToken = default)
        => await Context.Set<TModel>().CountAsync(criteria, cancellationToken);

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
