using Catalog.Api.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Catalog.Api.Data.Repositories;

public sealed class ProductsRepository
{
    private readonly DapperContext _ctx;

    public ProductsRepository(DapperContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<IEnumerable<object>> GetAllAsync()
    {
        using var conn = _ctx.CreateConnection();

        const string sql = """
            SELECT
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Stock,
                p.CategoryId,
                c.Name AS CategoryName,
                p.ImageUrl,
                p.CreatedAtUtc
            FROM Products p
            INNER JOIN Categories c ON c.Id = p.CategoryId
            ORDER BY p.Id DESC;
        """;

        return await conn.QueryAsync(sql);
    }

    public async Task<IEnumerable<object>> GetByCategoryIdAsync(int categoryId)
    {
        using var conn = _ctx.CreateConnection();

        const string sql = """
            SELECT
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Stock,
                p.CategoryId,
                c.Name AS CategoryName,
                p.ImageUrl,
                p.CreatedAtUtc
            FROM Products p
            INNER JOIN Categories c ON c.Id = p.CategoryId
            WHERE p.CategoryId = @CategoryId
            ORDER BY p.Id DESC;
        """;

        return await conn.QueryAsync(sql, new { CategoryId = categoryId });
    }

    public async Task<object?> GetByIdAsync(int id)
    {
        using var conn = _ctx.CreateConnection();

        const string sql = """
            SELECT
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Stock,
                p.CategoryId,
                c.Name AS CategoryName,
                p.ImageUrl,
                p.CreatedAtUtc
            FROM Products p
            INNER JOIN Categories c ON c.Id = p.CategoryId
            WHERE p.Id = @Id;
        """;

        return await conn.QueryFirstOrDefaultAsync(sql, new { Id = id });
    }

    // Returns created row, or null if CategoryId FK is invalid
    public async Task<object?> CreateAsync(
        string name,
        string description,
        decimal price,
        int categoryId,
        string? imageUrl,
        int stock
    )
    {
        using var conn = _ctx.CreateConnection();

        const string sql = """
            INSERT INTO Products (Name, Description, Price, Stock, CategoryId, ImageUrl)
            OUTPUT
                INSERTED.Id,
                INSERTED.Name,
                INSERTED.Description,
                INSERTED.Price,
                INSERTED.Stock,
                INSERTED.CategoryId,
                INSERTED.ImageUrl,
                INSERTED.CreatedAtUtc
            VALUES (@Name, @Description, @Price, @Stock, @CategoryId, @ImageUrl);
        """;

        try
        {
            return await conn.QuerySingleAsync(sql, new
            {
                Name = name,
                Description = description,
                Price = price,
                Stock = stock,
                CategoryId = categoryId,
                ImageUrl = imageUrl
            });
        }
        catch (SqlException ex) when (ex.Number == 547) // FK violation
        {
            // CategoryId doesn't exist
            return null;
        }
    }

    // Returns updated row, or null if Product not found or CategoryId FK invalid
    public async Task<object?> UpdateAsync(
        int id,
        string name,
        string? description,
        decimal price,
        int categoryId,
        string? imageUrl,
        int stock
    )
    {
        using var conn = _ctx.CreateConnection();

        const string sql = """
            UPDATE Products
            SET
                Name = @Name,
                Description = @Description,
                Price = @Price,
                Stock = @Stock,
                CategoryId = @CategoryId,
                ImageUrl = @ImageUrl
            OUTPUT
                INSERTED.Id,
                INSERTED.Name,
                INSERTED.Description,
                INSERTED.Price,
                INSERTED.Stock,
                INSERTED.CategoryId,
                INSERTED.ImageUrl,
                INSERTED.CreatedAtUtc
            WHERE Id = @Id;
        """;

        try
        {
            return await conn.QueryFirstOrDefaultAsync(sql, new
            {
                Id = id,
                Name = name,
                Description = description,
                Price = price,
                Stock = stock,
                CategoryId = categoryId,
                ImageUrl = imageUrl
            });
        }
        catch (SqlException ex) when (ex.Number == 547) // FK violation
        {
            // CategoryId doesn't exist
            return null;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _ctx.CreateConnection();

        const string sql = """
            DELETE FROM Products
            WHERE Id = @Id;
        """;

        var rows = await conn.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
}
