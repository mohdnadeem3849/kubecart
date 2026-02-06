using Catalog.Api.Data;
using Dapper;

namespace Catalog.Api.Data.Repositories;

public sealed class CategoriesRepository
{
    private readonly DapperContext _ctx;

    public CategoriesRepository(DapperContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<IEnumerable<object>> GetAllAsync()
    {
        using var conn = _ctx.CreateConnection();
        const string sql = """
            SELECT Id, Name
            FROM Categories
            ORDER BY Id DESC;
        """;

        return await conn.QueryAsync(sql);
    }

    public async Task<object?> GetByIdAsync(int id)
    {
        using var conn = _ctx.CreateConnection();
        const string sql = """
            SELECT Id, Name
            FROM Categories
            WHERE Id = @Id;
        """;

        return await conn.QueryFirstOrDefaultAsync(sql, new { Id = id });
    }

    public async Task<object> CreateAsync(string name)
    {
        using var conn = _ctx.CreateConnection();
        const string sql = """
            INSERT INTO Categories (Name)
            OUTPUT INSERTED.Id, INSERTED.Name
            VALUES (@Name);
        """;

        return await conn.QuerySingleAsync(sql, new { Name = name });
    }

    public async Task<object?> UpdateAsync(int id, string name)
    {
        using var conn = _ctx.CreateConnection();
        const string sql = """
            UPDATE Categories
            SET Name = @Name
            OUTPUT INSERTED.Id, INSERTED.Name
            WHERE Id = @Id;
        """;

        return await conn.QueryFirstOrDefaultAsync(sql, new { Id = id, Name = name });
    }

    // ✅ NEW: check if any product references this category
    public async Task<bool> HasProductsAsync(int categoryId)
    {
        using var conn = _ctx.CreateConnection();

        const string sql = """
            SELECT COUNT(1)
            FROM Products
            WHERE CategoryId = @Id;
        """;

        var count = await conn.ExecuteScalarAsync<int>(sql, new { Id = categoryId });
        return count > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _ctx.CreateConnection();
        const string sql = """
            DELETE FROM Categories
            WHERE Id = @Id;
        """;

        var rows = await conn.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
}
