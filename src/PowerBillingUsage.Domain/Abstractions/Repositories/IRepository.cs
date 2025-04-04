namespace PowerBillingUsage.Domain.Abstractions.Repositories;


public interface IRepository<Entity, EntityId> : IWriteRepository<Entity, EntityId>
    where Entity : class, IEntity<EntityId>
    where EntityId : IEntityId
{
}