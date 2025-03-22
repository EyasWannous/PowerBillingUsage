using System.Linq.Expressions;

namespace PowerBillingUsage.Domain.Abstractions;

public interface IBaseRepository<Entity, EntityId> : IDisposable
    where Entity : class, IEntity<EntityId>
    where EntityId : IEntityId
{
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<Entity, bool>> criteria, CancellationToken cancellationToken = default);
    Task DeleteAsync(EntityId id, CancellationToken cancellationToken = default);
    Task<Entity?> GetByIdAsync(EntityId id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Entity>> GetlistAsync(CancellationToken cancellationToken = default);
    Task<Entity> InsertAsync(Entity bill, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Entity> UpdateAsync(Entity bill, CancellationToken cancellationToken = default);
}
