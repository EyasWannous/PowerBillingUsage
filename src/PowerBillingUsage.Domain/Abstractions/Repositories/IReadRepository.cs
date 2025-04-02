using PowerBillingUsage.Domain.Abstractions.Shared;
using System.Linq.Expressions;

namespace PowerBillingUsage.Domain.Abstractions.Repositories;

public interface IReadRepository<ReadModel, EntityId> : IDisposable
    where ReadModel : class, IReadModel<EntityId>
    where EntityId : IEntityId
{
    Task<int> CountAsync(TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<ReadModel, bool>> criteria, CancellationToken cancellationToken = default);
    Task<ReadModel?> GetByIdAsync(EntityId id, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReadModel>> GetlistAsync(TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<PaingationResponse<ReadModel>> GetPaginateAsync(int skip, int take, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<IQueryable<ReadModel>> GetQueryableAsync();
    Task<IQueryable<ReadModel>> GetQueryableWithDetailsAsync(params List<Expression<Func<ReadModel, object>>>? navigationPropertyPaths);
}
