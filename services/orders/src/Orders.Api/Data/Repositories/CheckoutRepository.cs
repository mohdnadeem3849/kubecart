using Dapper;
using Orders.Api.Clients;
using Orders.Api.Data;
using Orders.Api.Models;

namespace Orders.Api.Data.Repositories;

public class CheckoutRepository
{
    private readonly DapperContext _ctx;
    private readonly CatalogClient _catalog;

    public CheckoutRepository(DapperContext ctx, CatalogClient catalog)
    {
        _ctx = ctx;
        _catalog = catalog;
    }

    public async Task<Order> CheckoutAsync(Guid userId, string notes)
    {
        using var conn = _ctx.CreateConnection();
        await conn.OpenAsync();
        using var tx = conn.BeginTransaction();

        try
        {
            // 1) Load cart items
            var cartItems = (await conn.QueryAsync<CartItem>(
                "SELECT Id, UserId, ProductId, Quantity, CreatedAtUtc FROM dbo.CartItems WHERE UserId=@UserId;",
                new { UserId = userId },
                transaction: tx
            )).ToList();

            if (cartItems.Count == 0)
                throw new InvalidOperationException("Cart is empty. Add items before checkout.");

            // 2) Fetch product details from Catalog + compute total
            decimal totalAmount = 0m;

            // create order id first
            var orderId = Guid.NewGuid();
            var createdAtUtc = DateTime.UtcNow;

            // 3) Insert order (TotalAmount will be updated after items inserted)
            await conn.ExecuteAsync(
                """
                INSERT INTO dbo.Orders (Id, UserId, Notes, Status, TotalAmount, CreatedAtUtc)
                VALUES (@Id, @UserId, @Notes, 'Pending', 0, @CreatedAtUtc);
                """,
                new
                {
                    Id = orderId,
                    UserId = userId,
                    Notes = notes ?? "",
                    CreatedAtUtc = createdAtUtc
                },
                transaction: tx
            );

            // 4) Insert order items with snapshots
            foreach (var ci in cartItems)
            {
                var product = await _catalog.GetProductAsync(ci.ProductId);

                if (product is null)
                    throw new InvalidOperationException($"Product {ci.ProductId} not found in Catalog.");

                var unitPrice = product.Price;
                var lineTotal = unitPrice * ci.Quantity;
                totalAmount += lineTotal;

                await conn.ExecuteAsync(
                    """
                    INSERT INTO dbo.OrderItems
                    (Id, OrderId, ProductId, Quantity, ProductNameSnapshot, UnitPriceSnapshot, ImageUrlSnapshot)
                    VALUES
                    (NEWID(), @OrderId, @ProductId, @Quantity, @ProductName, @UnitPrice, @ImageUrl);
                    """,
                    new
                    {
                        OrderId = orderId,
                        ProductId = ci.ProductId,
                        Quantity = ci.Quantity,
                        ProductName = product.Name ?? "Unknown",
                        UnitPrice = unitPrice,
                        ImageUrl = product.ImageUrl ?? ""
                    },
                    transaction: tx
                );
            }

            // 5) Update Orders.TotalAmount (now we know total)
            await conn.ExecuteAsync(
                "UPDATE dbo.Orders SET TotalAmount=@TotalAmount WHERE Id=@OrderId;",
                new { TotalAmount = totalAmount, OrderId = orderId },
                transaction: tx
            );

            // 6) Clear cart
            await conn.ExecuteAsync(
                "DELETE FROM dbo.CartItems WHERE UserId=@UserId;",
                new { UserId = userId },
                transaction: tx
            );

            tx.Commit();

            // 7) return order
            return new Order
            {
                Id = orderId,
                UserId = userId,
                Notes = notes ?? "",
                Status = "Pending",
                TotalAmount = totalAmount,
                CreatedAtUtc = createdAtUtc
            };
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}
