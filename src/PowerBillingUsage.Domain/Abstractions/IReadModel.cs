namespace PowerBillingUsage.Domain.Abstractions;

public interface IReadModel<EntityId> where EntityId : IEntityId
{
    EntityId Id { get; }
}