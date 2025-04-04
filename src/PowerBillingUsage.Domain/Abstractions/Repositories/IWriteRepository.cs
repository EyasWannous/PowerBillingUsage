namespace PowerBillingUsage.Domain.Abstractions.Repositories;

public interface IWriteRepository<Entity, EntityId> : IBaseRepository<Entity, EntityId>
    where Entity : class, IEntity<EntityId>
    where EntityId : IEntityId
{
    Task DeleteAsync(EntityId id, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<Entity> InsertAsync(Entity entity, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Entity> UpdateAsync(Entity entity, CancellationToken cancellationToken = default);
}