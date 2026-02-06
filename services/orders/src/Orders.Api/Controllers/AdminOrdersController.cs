using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orders.Api.Data.Repositories;

namespace Orders.Api.Controllers;

[ApiController]
[Route("api/orders/admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminOrdersController : ControllerBase
{
    private readonly OrdersRepository _orders;

    public AdminOrdersController(OrdersRepository orders)
    {
        _orders = orders;
    }

    public record UpdateStatusRequest(string Status);

    // PUT /api/orders/admin/{orderId}/status
    [HttpPut("{orderId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid orderId, [FromBody] UpdateStatusRequest req)
    {
        var status = (req.Status ?? "").Trim();
        if (string.IsNullOrWhiteSpace(status))
            return BadRequest(new { message = "Status is required" });

        // Optional: allow only known statuses (keep minimal)
        var allowed = new[] { "Pending", "Paid", "Shipped", "Delivered", "Cancelled" };
        if (!allowed.Contains(status, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { message = $"Invalid status. Allowed: {string.Join(", ", allowed)}" });

        // Normalize (store nice casing)
        status = allowed.First(x => x.Equals(status, StringComparison.OrdinalIgnoreCase));

        var ok = await _orders.UpdateOrderStatusAsync(orderId, status);
        if (!ok) return NotFound(new { message = "Order not found" });

        return Ok(new { message = "Order status updated", orderId, status });
    }
}
