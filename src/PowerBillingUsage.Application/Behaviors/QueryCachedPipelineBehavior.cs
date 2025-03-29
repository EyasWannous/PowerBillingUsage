using MediatR;
using PowerBillingUsage.Application.Abstractions.Messaging;
using PowerBillingUsage.Domain.Abstractions.Services;
using PowerBillingUsage.Domain.Abstractions.Shared;

namespace PowerBillingUsage.Application.Behaviors;

public sealed class QueryCachedPipelineBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICachedQuery
{
    private readonly ICacheService _cacheService;

    public QueryCachedPipelineBehavior(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        return await _cacheService.GetOrCreateAsync(
            request.Key,
            _ => next(),
            request.Expiration,
            cancellationToken
        );
    }
}
