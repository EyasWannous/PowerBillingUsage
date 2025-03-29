using System.Linq.Expressions;

namespace PowerBillingUsage.Domain.Abstractions.Repositories;

public interface IRepository<Entity, EntityId> : IDisposable
    where Entity : class, IEntity<EntityId>
    where EntityId : IEntityId
{
    Task<int> CountAsync(TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<Entity, bool>> criteria, CancellationToken cancellationToken = default);
    Task DeleteAsync(EntityId id, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<Entity?> GetByIdAsync(EntityId id, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Entity>> GetlistAsync(TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Entity>> GetPaginateAsync(int skip, int take, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<Entity> InsertAsync(Entity bill, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Entity> UpdateAsync(Entity bill, CancellationToken cancellationToken = default);
}
