using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orders.Api.Data.Repositories;
using System.Security.Claims;

namespace Orders.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class CheckoutController : ControllerBase
{
    private readonly CheckoutRepository _repo;

    public CheckoutController(CheckoutRepository repo)
    {
        _repo = repo;
    }

    public record CheckoutRequest(string? Notes);

    [HttpPost("checkout")]
    [Authorize]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest req)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Unauthorized(new { message = "Invalid user id in token" });

        var order = await _repo.CheckoutAsync(userId, req.Notes ?? "");
        return Ok(order);
    }
}
