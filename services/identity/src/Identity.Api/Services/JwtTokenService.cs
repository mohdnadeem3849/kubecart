using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.Api.Services;

public sealed class JwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public string CreateToken(Guid userId, string email, string role)
    {
        var jwt = _config.GetSection("Jwt");
        var issuer = jwt["Issuer"];
        var audience = jwt["Audience"];
        var key = jwt["Key"];

        if (string.IsNullOrWhiteSpace(issuer) ||
            string.IsNullOrWhiteSpace(audience) ||
            string.IsNullOrWhiteSpace(key))
        {
            throw new Exception("Missing Jwt settings in appsettings.json (Jwt:Issuer, Jwt:Audience, Jwt:Key).");
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        // IMPORTANT: include BOTH standard + .NET claim types so all consumers work
        var claims = new List<Claim>
        {
            // Standard JWT subject
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),

            // .NET-friendly name id
            new(ClaimTypes.NameIdentifier, userId.ToString()),

            // Email (standard + .NET-friendly)
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Email, email),

            // Role (ClaimTypes.Role maps to the URI your token shows)
            new(ClaimTypes.Role, role),

            // Unique token id
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
