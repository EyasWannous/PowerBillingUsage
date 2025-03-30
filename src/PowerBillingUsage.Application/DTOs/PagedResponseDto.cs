namespace PowerBillingUsage.Application.DTOs;

public record PagedResponseDto<T>(List<T> Values, int TotalCount);
