﻿using PowerBillingUsage.Domain.Abstractions;

namespace PowerBillingUsage.Domain.Tiers;

public record TierId(Guid Value) : IEntityId;
