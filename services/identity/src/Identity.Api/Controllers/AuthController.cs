using System.Security.Claims;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Identity.Api.Services;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly JwtTokenService _jwt;

    public AuthController(IConfiguration config, JwtTokenService jwt)
    {
        _config = config;
        _jwt = jwt;
    }

    // ---------- DTOs ----------
    public record RegisterRequest(string Email, string Password, string? Role);
    public record LoginRequest(string Email, string Password);

    private sealed record UserRow(Guid Id, string Email, string Role, string PasswordHash);

    private SqlConnection CreateConn()
    {
        var cs = _config.GetConnectionString("IdentityDb");
        if (string.IsNullOrWhiteSpace(cs))
            throw new Exception("Missing connection string: ConnectionStrings:IdentityDb");

        return new SqlConnection(cs);
    }

    private static string NormalizeEmail(string email) =>
        (email ?? "").Trim().ToLowerInvariant();

    // ---------- GET /api/auth/debug/db ----------
    [HttpGet("debug/db")]
    [AllowAnonymous]
    public async Task<IResult> DebugDb()
    {
        try
        {
            await using var conn = CreateConn();
            await conn.OpenAsync();

            var server = await conn.ExecuteScalarAsync<string>("SELECT @@SERVERNAME");
            var db = await conn.ExecuteScalarAsync<string>("SELECT DB_NAME()");

            return Results.Ok(new { connected = true, server, db });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                title: "Database connection failed",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    // ---------- POST /api/auth/register ----------
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var email = NormalizeEmail(req.Email);

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { message = "Email and password are required." });

        // role default to Customer if not provided
        var role = string.IsNullOrWhiteSpace(req.Role) ? "Customer" : req.Role.Trim();

        try
        {
            await using var conn = CreateConn();
            await conn.OpenAsync();

            var exists = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM dbo.Users WHERE Email = @Email",
                new { Email = email });

            if (exists > 0)
                return Conflict(new { message = "Email already registered." });

            var id = Guid.NewGuid();

            // NOTE: plaintext for learning. Replace with BCrypt later.
            var passwordHash = req.Password;

            await conn.ExecuteAsync(@"
INSERT INTO dbo.Users (Id, Email, Role, PasswordHash, CreatedAtUtc)
VALUES (@Id, @Email, @Role, @PasswordHash, SYSUTCDATETIME());",
                new { Id = id, Email = email, Role = role, PasswordHash = passwordHash });

            return Ok(new { message = "Registered", id, email, role });
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message, title: "Database error during register.", statusCode: 500);
        }
    }

    // ---------- POST /api/auth/login ----------
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var email = NormalizeEmail(req.Email);

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { message = "Email and password are required." });

        try
        {
            await using var conn = CreateConn();
            await conn.OpenAsync();

            var user = await conn.QuerySingleOrDefaultAsync<UserRow>(@"
SELECT TOP 1 Id, Email, Role, PasswordHash
FROM dbo.Users
WHERE Email = @Email;",
                new { Email = email });

            if (user is null)
                return Unauthorized(new { message = "Invalid email or password." });

            var ok = string.Equals(req.Password, user.PasswordHash, StringComparison.Ordinal);
            if (!ok)
                return Unauthorized(new { message = "Invalid email or password." });

            var token = _jwt.CreateToken(user.Id, user.Email, user.Role);

            return Ok(new
            {
                accessToken = token,
                user = new { user.Id, user.Email, user.Role }
            });
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message, title: "Database error during login.", statusCode: 500);
        }
    }

    // ---------- GET /api/auth/me ----------
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
        var role = User.FindFirstValue(ClaimTypes.Role);

        return Ok(new { userId, email, role });
    }
}
