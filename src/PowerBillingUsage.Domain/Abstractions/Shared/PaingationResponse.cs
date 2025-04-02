namespace PowerBillingUsage.Domain.Abstractions.Shared;

public record PaingationResponse<Entity>(int TotalCount, IEnumerable<Entity> Items);