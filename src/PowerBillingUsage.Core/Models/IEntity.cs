namespace PowerBillingUsage.Core.Models;

public interface IEntity<EntityId> where EntityId : IEntityId
{
    EntityId Id { get; }
}
