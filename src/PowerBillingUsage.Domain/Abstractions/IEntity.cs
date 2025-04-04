namespace PowerBillingUsage.Domain.Abstractions;

public interface IEntity<TEntityId> : IHasId<TEntityId>
    where TEntityId : IEntityId;