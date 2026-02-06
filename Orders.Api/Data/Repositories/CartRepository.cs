using Dapper;
using Orders.Api.Data;

namespace Orders.Api.Data.Repositories;

public sealed class CartRepository
{
    private readonly DapperContext _ctx;

    public CartRepository(DapperContext ctx) => _ctx = ctx;

    public async Task<IEnumerable<object>> GetCartAsync(Guid userId)
    {
        using var conn = _ctx.CreateConnection();

        const string sql = """
            SELECT Id, UserId, ProductId, Quantity, CreatedAtUtc
            FROM CartItems
            WHERE UserId = @UserId
            ORDER BY Id DESC;
        """;

        return await conn.QueryAsync(sql, new { UserId = userId });
    }

    public async Task<object> AddItemAsync(Guid userId, int productId, int quantity)
    {
        using var conn = _ctx.CreateConnection();

        // If same product exists, increment quantity
        const string sql = """
            IF EXISTS (SELECT 1 FROM CartItems WHERE UserId=@UserId AND ProductId=@ProductId)
            BEGIN
                UPDATE CartItems
                SET Quantity = Quantity + @Quantity
                OUTPUT INSERTED.Id, INSERTED.UserId, INSERTED.ProductId, INSERTED.Quantity, INSERTED.CreatedAtUtc
                WHERE UserId=@UserId AND ProductId=@ProductId;
            END
            ELSE
            BEGIN
                INSERT INTO CartItems (UserId, ProductId, Quantity)
                OUTPUT INSERTED.Id, INSERTED.UserId, INSERTED.ProductId, INSERTED.Quantity, INSERTED.CreatedAtUtc
                VALUES (@UserId, @ProductId, @Quantity);
            END
        """;

        return await conn.QuerySingleAsync(sql, new { UserId = userId, ProductId = productId, Quantity = quantity });
    }

    public async Task<IEnumerable<(int ProductId, int Quantity)>> GetCartProductLinesAsync(Guid userId)
    {
        using var conn = _ctx.CreateConnection();

        const string sql = """
            SELECT ProductId, Quantity
            FROM CartItems
            WHERE UserId = @UserId;
        """;

        var rows = await conn.QueryAsync(sql, new { UserId = userId });
        return rows.Select(r => ((int)r.ProductId, (int)r.Quantity));
    }
    public async Task<int> RemoveCartItemAsync(Guid userId, Guid cartItemId)
    {
        using var conn = _ctx.CreateConnection();
        const string sql = "DELETE FROM dbo.CartItems WHERE Id=@Id AND UserId=@UserId;";
        return await conn.ExecuteAsync(sql, new { Id = cartItemId, UserId = userId });
    }

    public async Task ClearCartAsync(Guid userId)
    {
        using var conn = _ctx.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM CartItems WHERE UserId = @UserId;", new { UserId = userId });
    }
}
