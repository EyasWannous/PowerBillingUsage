﻿using PowerBillingUsage.Domain.Abstractions;

namespace PowerBillingUsage.Domain.Bills;

public record BillId(Guid Id) : IEntityId;
