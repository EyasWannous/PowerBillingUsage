namespace PowerBillingUsage.Domain.Abstractions.Repositories;

public interface IWriteRepository<Entity, EntityId> : IBaseRepository<Entity, EntityId>
    where Entity : class, IEntity<EntityId>
    where EntityId : IEntityId
{
    Task<int> DeleteAndSaveChangesAsync(Entity item, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task DeleteAsync(EntityId id, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<int> DeleteRangeAndSaveChangesAsync(List<Entity> items, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(List<EntityId> itemsIds, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<Entity> InsertAsync(Entity entity, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Entity> UpdateAsync(Entity entity, CancellationToken cancellationToken = default);
}