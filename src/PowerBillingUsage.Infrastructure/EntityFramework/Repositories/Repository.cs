using PowerBillingUsage.Domain.Abstractions;
using PowerBillingUsage.Domain.Abstractions.Helpers;
using PowerBillingUsage.Domain.Abstractions.RegisteringDependencies;
using PowerBillingUsage.Domain.Abstractions.Repositories;
using PowerBillingUsage.Domain.Abstractions.Services;

namespace PowerBillingUsage.Infrastructure.EntityFramework.Repositories;

public class Repository<Entity, EntityId> :
    WriteRepository<Entity, EntityId>,
    IRepository<Entity, EntityId>,
    IScopedDependency
    where Entity : class, IEntity<EntityId>
    where EntityId : IEntityId
{
    public Repository(
        PowerBillingUsageWriteDbContext context,
        IHybridCacheService cacheService,
        ICacheKeyHelper<Entity> cacheKeyHelper)
        : base(context, cacheService, cacheKeyHelper)
    {
    }
}