using PowerBillingUsage.Domain.Abstractions.Shared;
using System.Linq.Expressions;

namespace PowerBillingUsage.Domain.Abstractions.Repositories;

public interface IBaseRepository<TModel, TEntityId> : IDisposable
    where TModel : class, IHasId<TEntityId>
    where TEntityId : IEntityId
{
    Task<int> CountAsync(TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<TModel, bool>> criteria, CancellationToken cancellationToken = default);
    Task<TModel?> GetByIdAsync(TEntityId id, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<TModel>> GetListAsync(TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<PaingationResponse<TModel>> GetPaginateAsync(int skip, int take, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<IQueryable<TModel>> GetQueryableAsync();
    Task<IQueryable<TModel>> GetQueryableWithDetailsAsync(params List<Expression<Func<TModel, object>>>? navigationPropertyPaths);
}
