namespace PowerBillingUsage.Domain.Abstractions.Repositories;

public interface IReadRepository<ReadModel, EntityId> : IBaseRepository<ReadModel, EntityId>
    where ReadModel : class, IReadModel<EntityId>
    where EntityId : IEntityId
{
}