namespace PowerBillingUsage.Domain.Abstractions;

public interface IBaseRepository<Entity, EntityId> : IDisposable
    where Entity : class, IEntity<EntityId>
    where EntityId : IEntityId
{
    Task DeleteAsync(EntityId id);
    Task<Entity?> GetByIdAsync(EntityId id);
    Task<IEnumerable<Entity>> GetlistAsync();
    Task InsertAsync(Entity bill);
    Task<int> SaveChangesAsync();
    Task UpdateAsync(Entity bill);
}
