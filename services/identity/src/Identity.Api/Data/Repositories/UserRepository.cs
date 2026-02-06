using Dapper;
using Identity.Api.Models;
using Microsoft.Data.SqlClient;

namespace Identity.Api.Data.Repositories;

public class UserRepository
{
    private readonly Identity.Api.Data.DapperContext _ctx;

    public UserRepository(Identity.Api.Data.DapperContext ctx)
    {
        _ctx = ctx;
    }

    // ✅ Matches your DB: Users(Id uniqueidentifier, Email, PasswordHash, Role, CreatedAtUtc)
    public async Task<User?> GetByEmailAsync(string email)
    {
        const string sql = @"
SELECT TOP 1 Id, Email, PasswordHash, Role, CreatedAtUtc
FROM dbo.Users
WHERE Email = @Email;
";
        using var conn = _ctx.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        const string sql = @"
SELECT TOP 1 Id, Email, PasswordHash, Role, CreatedAtUtc
FROM dbo.Users
WHERE Id = @Id;
";
        using var conn = _ctx.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<Guid> CreateUserAsync(string email, string passwordHash, string role)
    {
        var newId = Guid.NewGuid();

        const string sql = @"
INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, CreatedAtUtc)
VALUES (@Id, @Email, @PasswordHash, @Role, SYSUTCDATETIME());
";

        using var conn = _ctx.CreateConnection();
        await conn.ExecuteAsync(sql, new
        {
            Id = newId,
            Email = email,
            PasswordHash = passwordHash,
            Role = role
        });

        return newId;
    }

    public async Task InsertAuditAsync(Guid? userId, string action, string detail)
    {
        const string sql = @"
INSERT INTO dbo.AuditLogs (Id, Action, UserId, TimestampUtc, Detail)
VALUES (NEWID(), @Action, @UserId, SYSUTCDATETIME(), @Detail);
";
        using var conn = _ctx.CreateConnection();
        await conn.ExecuteAsync(sql, new { Action = action, UserId = userId, Detail = detail });
    }
}
