using Microsoft.EntityFrameworkCore;
using PowerBillingUsage.Domain.Abstractions;
using PowerBillingUsage.Domain.Abstractions.RegisteringDependencies;
using PowerBillingUsage.Domain.Abstractions.Repositories;
using PowerBillingUsage.Domain.Abstractions.Services;
using System.Linq.Expressions;

namespace PowerBillingUsage.Infrastructure.EntityFramework.Repositories;

public class ReadRepository<ReadModel, EntityId> : IReadRepository<ReadModel, EntityId>, IScopedDependency, IDisposable
    where ReadModel : class, IReadModel<EntityId>
    where EntityId : IEntityId
{
    protected readonly PowerBillingUsageReadDbContext Context;
    protected readonly ICacheService CacheService;
    protected readonly string KeyAll = $"allOf_{typeof(ReadModel)}";
    protected readonly string PaginateKey = $"paginate_{typeof(ReadModel)}_";
    protected readonly string KeyOne = $"oneOf_{typeof(ReadModel)}_Id: ";
    protected readonly string CountKey = $"count_{typeof(ReadModel)}";

    public ReadRepository(PowerBillingUsageReadDbContext context, ICacheService cacheService)
    {
        Context = context;
        CacheService = cacheService;
    }

    public async Task<IEnumerable<ReadModel>> GetPaginateAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        var paginateKey = MakePaginateKey(skip, take);

        var items = await CacheService.GetAsync<IEnumerable<ReadModel>>(paginateKey, cancellationToken);
        if (items is not null)
            return items;

        items = await Context.Set<ReadModel>()
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync(cancellationToken: cancellationToken);

        await CacheService.SetAsync(paginateKey, items, cancellationToken);

        return items;
    }

    public async Task<IEnumerable<ReadModel>> GetlistAsync(CancellationToken cancellationToken = default)
    {
        var items = await CacheService.GetAsync<IEnumerable<ReadModel>>(KeyAll, cancellationToken);
        if (items is not null)
            return items;

        items = await Context.Set<ReadModel>()
            .AsNoTracking()
            .ToListAsync(cancellationToken: cancellationToken);

        await CacheService.SetAsync(KeyAll, items, cancellationToken);

        return items;
    }

    public async Task<ReadModel?> GetByIdAsync(EntityId id, CancellationToken cancellationToken = default)
    {
        var keyOne = MakeKeyOne(id);

        var item = await CacheService.GetAsync<ReadModel>(keyOne, cancellationToken);
        if (item is not null)
            return item;

        item = await Context.Set<ReadModel>().FirstOrDefaultAsync(x => x.Id.Value == id.Value, cancellationToken);
        if (item is null)
            return null;

        await CacheService.SetAsync(keyOne, item, cancellationToken);

        return item;
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        var count = await CacheService.GetAsync<int>(CountKey, cancellationToken);
        if (count is not 0)
            return count;

        count = await Context.Set<ReadModel>().CountAsync(cancellationToken);

        await CacheService.SetAsync(CountKey, count, cancellationToken);

        return count;
    }

    public async Task<int> CountAsync(Expression<Func<ReadModel, bool>> criteria, CancellationToken cancellationToken = default)
        => await Context.Set<ReadModel>().CountAsync(criteria, cancellationToken);

    private string MakeKeyOne(EntityId id) => KeyOne + id.Value.ToString();

    private string MakePaginateKey(int skip, int take)
        => PaginateKey + "skip: " + skip + "_take: " + take;

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
