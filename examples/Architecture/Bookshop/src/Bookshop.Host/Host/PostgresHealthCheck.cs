using Bookshop.Infrastructure.Common.Database;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bookshop.Host.Host;

/// <summary>
/// Reports whether the Bookshop database is reachable. Tagged "ready" only: an unreachable
/// database should fail readiness (take the instance out of load-balancing) without also
/// failing liveness (which would restart a process that is otherwise fine).
/// </summary>
internal sealed class PostgresHealthCheck(BookshopContext context) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext healthCheckContext,
        CancellationToken cancellationToken)
    {
        try
        {
            return await context.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("The database is available.")
                : HealthCheckResult.Unhealthy("The database is not available.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("The database is not available.", ex);
        }
    }
}
