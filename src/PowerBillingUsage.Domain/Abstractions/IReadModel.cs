namespace PowerBillingUsage.Domain.Abstractions;

public interface IReadModel<TEntityId> : IHasId<TEntityId>
    where TEntityId : IEntityId;