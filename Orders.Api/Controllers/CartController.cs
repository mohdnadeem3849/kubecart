using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orders.Api.Data.Repositories;
using Orders.Api.Models.Dtos;
using System.Security.Claims;

namespace Orders.Api.Controllers;

[ApiController]
[Route("api/orders/cart")]
public sealed class CartController : ControllerBase
{
    private readonly CartRepository _cartRepo;

    public CartController(CartRepository cartRepo) => _cartRepo = cartRepo;

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(sub)) throw new Exception("Missing user id claim.");
        return Guid.Parse(sub);
    }

    // GET /api/orders/cart
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = GetUserId();
        var items = await _cartRepo.GetCartAsync(userId);
        return Ok(items);
    }
    // DELETE /api/orders/cart/items/{id}
    [HttpDelete("items/{id:guid}")]
    public async Task<IActionResult> RemoveItem(Guid id)
    {
        var userId = GetUserId(); // same method you already have
        var rows = await _cartRepo.RemoveCartItemAsync(userId, id);

        return rows == 0 ? NotFound() : NoContent();
    }

    // POST /api/orders/cart/items
    [Authorize]
    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest req)
    {
        if (req == null) return BadRequest(new { message = "body is required" });
        if (req.ProductId <= 0) return BadRequest(new { message = "productId is required" });
        if (req.Quantity <= 0) return BadRequest(new { message = "quantity must be > 0" });

        var userId = GetUserId();
        var added = await _cartRepo.AddItemAsync(userId, req.ProductId, req.Quantity);
        return Ok(added);
    }
}
