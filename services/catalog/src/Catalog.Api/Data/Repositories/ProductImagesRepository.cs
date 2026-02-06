using Catalog.Api.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Catalog.Api.Data.Repositories;

public sealed class ProductImagesRepository
{
    private readonly DapperContext _ctx;

    public ProductImagesRepository(DapperContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<IEnumerable<object>> GetByProductIdAsync(int productId)
    {
        using var conn = _ctx.CreateConnection();

        const string sql = """
            SELECT Id, ProductId, Url, SortOrder
            FROM ProductImages
            WHERE ProductId = @ProductId
            ORDER BY SortOrder ASC, Id ASC;
        """;

        return await conn.QueryAsync(sql, new { ProductId = productId });
    }

    public async Task<object?> AddAsync(int productId, string url, bool isPrimary)
    {
        using var conn = _ctx.CreateConnection();

        try
        {
            // 🔹 Ensure product exists (important!)
            const string productExistsSql = """
                SELECT 1 FROM Products WHERE Id = @ProductId;
            """;

            var exists = await conn.ExecuteScalarAsync<int?>(
                productExistsSql,
                new { ProductId = productId }
            );

            if (exists == null)
                return null;

            int sortOrder;

            if (isPrimary)
            {
                // Push existing images down
                const string shiftSql = """
                    UPDATE ProductImages
                    SET SortOrder = SortOrder + 1
                    WHERE ProductId = @ProductId;
                """;

                await conn.ExecuteAsync(shiftSql, new { ProductId = productId });
                sortOrder = 0;
            }
            else
            {
                const string maxSql = """
                    SELECT ISNULL(MAX(SortOrder), -1) + 1
                    FROM ProductImages
                    WHERE ProductId = @ProductId;
                """;

                sortOrder = await conn.ExecuteScalarAsync<int>(
                    maxSql,
                    new { ProductId = productId }
                );
            }

            const string insertSql = """
                INSERT INTO ProductImages (ProductId, Url, SortOrder)
                OUTPUT INSERTED.Id, INSERTED.ProductId, INSERTED.Url, INSERTED.SortOrder
                VALUES (@ProductId, @Url, @SortOrder);
            """;

            return await conn.QuerySingleAsync(insertSql, new
            {
                ProductId = productId,
                Url = url,
                SortOrder = sortOrder
            });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            return null;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _ctx.CreateConnection();

        const string sql = """
            DELETE FROM ProductImages
            WHERE Id = @Id;
        """;

        return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
    }
}
