using System.Linq.Expressions;

namespace PowerBillingUsage.Domain.Abstractions.Repositories;

public interface IReadRepository<ReadModel, EntityId> : IDisposable
    where ReadModel : class, IReadModel<EntityId>
    where EntityId : IEntityId
{
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<ReadModel, bool>> criteria, CancellationToken cancellationToken = default);
    Task<ReadModel?> GetByIdAsync(EntityId id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReadModel>> GetlistAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ReadModel>> GetPaginateAsync(int skip, int take, CancellationToken cancellationToken = default);
}
