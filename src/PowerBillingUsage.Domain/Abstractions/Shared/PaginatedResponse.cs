namespace PowerBillingUsage.Domain.Abstractions.Shared;

public record PaginatedResponse<Entity>(int TotalCount, IEnumerable<Entity> Items);