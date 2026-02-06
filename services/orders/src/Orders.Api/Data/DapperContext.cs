using Microsoft.Data.SqlClient;

namespace Orders.Api.Data;

public class DapperContext
{
    private readonly IConfiguration _config;

    public DapperContext(IConfiguration config)
    {
        _config = config;
    }

    public SqlConnection CreateConnection()
    {
        // -------- 1) Try ENV vars first (production / Kubernetes) --------
        var host = Environment.GetEnvironmentVariable("DB_HOST");
        var db = Environment.GetEnvironmentVariable("DB_NAME");
        var user = Environment.GetEnvironmentVariable("DB_USER");
        var pass = Environment.GetEnvironmentVariable("DB_PASSWORD");

        if (!string.IsNullOrWhiteSpace(host) &&
            !string.IsNullOrWhiteSpace(db) &&
            !string.IsNullOrWhiteSpace(user) &&
            !string.IsNullOrWhiteSpace(pass))
        {
            var envCs = new SqlConnectionStringBuilder
            {
                DataSource = host,
                InitialCatalog = db,
                UserID = user,
                Password = pass,
                TrustServerCertificate = true,
                Encrypt = false
            }.ConnectionString;

            return new SqlConnection(envCs);
        }

        // -------- 2) Fallback to appsettings.json (LOCAL DEV SAFE) --------
        var cs = _config.GetConnectionString("OrdersDb");

        if (string.IsNullOrWhiteSpace(cs))
            throw new Exception(
                "Missing DB config. Set ENV vars (DB_HOST, DB_NAME, DB_USER, DB_PASSWORD) " +
                "or ConnectionStrings:OrdersDb in appsettings.json"
            );

        return new SqlConnection(cs);
    }
}
