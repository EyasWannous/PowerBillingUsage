using MediatR;
using PowerBillingUsage.Domain.Abstractions.Shared;

namespace PowerBillingUsage.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
