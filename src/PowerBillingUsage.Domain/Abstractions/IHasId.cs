namespace PowerBillingUsage.Domain.Abstractions;

public interface IHasId<TEntityId> where TEntityId : IEntityId
{
    TEntityId Id { get; }
}
