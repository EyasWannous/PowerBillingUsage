﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PowerBillingUsage.Infrastructure.EntityFramework;

namespace PowerBillingUsage.Infrastructure.Health;

public sealed class DatabaseHealthCheck(PowerBillingUsageWriteDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.Database.OpenConnectionAsync(cancellationToken);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(exception: ex);
        }
        finally
        {

            await dbContext.Database.CloseConnectionAsync();
        }
    }
}
