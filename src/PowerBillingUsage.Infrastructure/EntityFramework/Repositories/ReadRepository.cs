using PowerBillingUsage.Domain.Abstractions;
using PowerBillingUsage.Domain.Abstractions.Helpers;
using PowerBillingUsage.Domain.Abstractions.RegisteringDependencies;
using PowerBillingUsage.Domain.Abstractions.Repositories;
using PowerBillingUsage.Domain.Abstractions.Services;

namespace PowerBillingUsage.Infrastructure.EntityFramework.Repositories;

public class ReadRepository<ReadModel, EntityId> :
    BaseRepository<ReadModel, EntityId, PowerBillingUsageReadDbContext>,
    IReadRepository<ReadModel, EntityId>,
    IScopedDependency
    where ReadModel : class, IReadModel<EntityId>
    where EntityId : IEntityId
{
    public ReadRepository(
        PowerBillingUsageReadDbContext context,
        IHybridCacheService cacheService,
        ICacheKeyHelper<ReadModel> cacheKeyHelper)
        : base(context, cacheService, cacheKeyHelper)
    {
    }
}