using PowerBillingUsage.Domain.Abstractions;

namespace PowerBillingUsage.Domain.Bills;

public record BillDetailId(Guid Value) : IEntityId;
