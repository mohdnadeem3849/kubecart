using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("debug")]
[AllowAnonymous] // ✅ entire controller is public
public class DebugController : ControllerBase
{
    private readonly IConfiguration _cfg;

    public DebugController(IConfiguration cfg)
    {
        _cfg = cfg;
    }

    [HttpGet("jwt")]
    public IActionResult JwtInfo()
    {
        var jwt = _cfg.GetSection("Jwt");
        return Ok(new
        {
            issuer = jwt["Issuer"],
            audience = jwt["Audience"],
            keyLength = (jwt["Key"] ?? "").Length
        });
    }

    [HttpPost("validate-token")]
    public IActionResult ValidateToken()
    {
        var auth = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(auth) || !auth.StartsWith("Bearer "))
            return BadRequest(new { message = "Missing Authorization: Bearer <token>" });

        var token = auth["Bearer ".Length..].Trim();

        var issuer = _cfg["Jwt:Issuer"];
        var audience = _cfg["Jwt:Audience"];
        var key = _cfg["Jwt:Key"];

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!)),

                ValidateIssuer = true,
                ValidIssuer = issuer,

                ValidateAudience = true,
                ValidAudience = audience,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            }, out _);

            var claims = principal.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Ok(new { valid = true, claims });
        }
        catch (Exception ex)
        {
            return StatusCode(401, new
            {
                valid = false,
                error = ex.GetType().Name,
                message = ex.Message
            });
        }
    }
}
