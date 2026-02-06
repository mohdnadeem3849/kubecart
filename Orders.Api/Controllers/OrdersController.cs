using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orders.Api.Data.Repositories;
using System.Security.Claims;

namespace Orders.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrdersRepository _orders;

    public OrdersController(OrdersRepository orders)
    {
        _orders = orders;
    }

    private Guid GetUserId()
    {
        var userIdStr =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdStr, out var userId))
            throw new InvalidOperationException("Invalid user id claim.");

        return userId;
    }

    // GET /api/orders
    [HttpGet]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = GetUserId();
        var orders = await _orders.GetOrdersForUserAsync(userId);
        return Ok(orders);
    }

    // GET /api/orders/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var userId = GetUserId();
        var details = await _orders.GetOrderForUserByIdAsync(userId, id);
        if (details is null) return NotFound();
        return Ok(details);
    }
}
