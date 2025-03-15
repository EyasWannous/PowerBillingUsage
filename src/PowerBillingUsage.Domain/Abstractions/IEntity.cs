namespace PowerBillingUsage.Domain.Abstractions;

public interface IEntity<EntityId> where EntityId : IEntityId
{
    EntityId Id { get; }
}
