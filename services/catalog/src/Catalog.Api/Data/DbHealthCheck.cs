using System.Data;
using Dapper;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Catalog.Api.Data;

public sealed class DbHealthCheck : IHealthCheck
{
    private readonly DapperContext _ctx;

    public DbHealthCheck(DapperContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var conn = _ctx.CreateConnection();
            if (conn.State != ConnectionState.Open)
                conn.Open();

            var ok = await conn.ExecuteScalarAsync<int>("SELECT 1;");
            return ok == 1
                ? HealthCheckResult.Healthy("DB OK")
                : HealthCheckResult.Unhealthy("DB returned unexpected result");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("DB connection failed", ex);
        }
    }
}
