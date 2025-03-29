using MediatR;
using Microsoft.Extensions.Logging;
using PowerBillingUsage.Domain.Abstractions.Shared;

namespace PowerBillingUsage.Application.Behaviors;

public class LoggingPiplineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly ILogger<LoggingPiplineBehavior<TRequest, TResponse>> _logger;

    public LoggingPiplineBehavior(ILogger<LoggingPiplineBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting request {@RequestName}, {DateTimeUtc}",
            typeof(TRequest).Name,
            DateTime.UtcNow
        );

        var result = await next();
        
        _logger.LogInformation(
            "Completed request {@RequestName}, {DateTimeUtc}",
            typeof(TRequest).Name,
            DateTime.UtcNow
        );

        return result;
    }
}
