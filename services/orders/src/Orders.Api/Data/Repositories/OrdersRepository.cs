using Dapper;
using Orders.Api.Data;
using Orders.Api.Models;

namespace Orders.Api.Data.Repositories;

public class OrdersRepository
{
    private readonly DapperContext _ctx;

    public OrdersRepository(DapperContext ctx)
    {
        _ctx = ctx;
    }

    // GET /api/orders -> list orders for logged-in user
    public async Task<IEnumerable<Order>> GetOrdersForUserAsync(Guid userId)
    {
        using var conn = _ctx.CreateConnection();

        const string sql = """
            SELECT Id, UserId, Notes, Status, TotalAmount, CreatedAtUtc
            FROM dbo.Orders
            WHERE UserId = @UserId
            ORDER BY CreatedAtUtc DESC;
        """;

        return await conn.QueryAsync<Order>(sql, new { UserId = userId });
    }
    public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string status)
    {
        using var conn = _ctx.CreateConnection();
        const string sql = """
        UPDATE dbo.Orders
        SET Status = @Status
        WHERE Id = @OrderId;
    """;

        var rows = await conn.ExecuteAsync(sql, new { Status = status, OrderId = orderId });
        return rows > 0;
    }

    // GET /api/orders/{id} -> order + items (only if belongs to user)
    public async Task<OrderDetails?> GetOrderForUserByIdAsync(Guid userId, Guid orderId)
    {
        using var conn = _ctx.CreateConnection();

        const string orderSql = """
            SELECT Id, UserId, Notes, Status, TotalAmount, CreatedAtUtc
            FROM dbo.Orders
            WHERE Id = @OrderId AND UserId = @UserId;
        """;

        var order = await conn.QueryFirstOrDefaultAsync<Order>(
            orderSql, new { OrderId = orderId, UserId = userId });

        if (order is null) return null;

        // IMPORTANT: don't ORDER BY CreatedAtUtc unless your OrderItems table has that column
        const string itemsSql = """
            SELECT 
                Id,
                OrderId,
                ProductId,
                Quantity,
                ProductNameSnapshot,
                UnitPriceSnapshot,
                ImageUrlSnapshot
            FROM dbo.OrderItems
            WHERE OrderId = @OrderId
            ORDER BY Id ASC;
        """;

        var items = (await conn.QueryAsync<OrderItem>(itemsSql, new { OrderId = orderId })).AsList();

        return new OrderDetails
        {
            Order = order,
            Items = items
        };
    }
}

public class OrderDetails
{
    public required Order Order { get; set; }
    public required List<OrderItem> Items { get; set; }
}
