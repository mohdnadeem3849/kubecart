using Catalog.Api.Data;
using Catalog.Api.Data.Repositories;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger + JWT button
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "KubeCart Catalog API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste ONLY the JWT token. Swagger will send: Authorization: Bearer <token>"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// JWT settings (FAIL FAST if missing)
string Env(string key, string? fallback = null) =>
    Environment.GetEnvironmentVariable(key) ?? fallback ?? "";

var jwt = builder.Configuration.GetSection("Jwt");

var issuer = Env("JWT_ISSUER", jwt["Issuer"]);
var audience = Env("JWT_AUDIENCE", jwt["Audience"]);
var key = Env("JWT_KEY", jwt["Key"]);

if (string.IsNullOrWhiteSpace(issuer) ||
    string.IsNullOrWhiteSpace(audience) ||
    string.IsNullOrWhiteSpace(key))
{
    throw new Exception("Missing JWT settings. Set env vars (JWT_ISSUER/JWT_AUDIENCE/JWT_KEY) or appsettings Jwt section.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),

            ValidateIssuer = true,
            ValidIssuer = issuer,

            ValidateAudience = true,
            ValidAudience = audience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),

            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.NameIdentifier
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine("JWT AUTH FAILED: " + ctx.Exception.GetType().Name + " - " + ctx.Exception.Message);
                return Task.CompletedTask;
            },
            OnChallenge = ctx =>
            {
                Console.WriteLine("JWT CHALLENGE: " + ctx.Error + " - " + ctx.ErrorDescription);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// ? Register DB + repositories
builder.Services.AddScoped<DapperContext>();
builder.Services.AddScoped<CategoriesRepository>();
builder.Services.AddScoped<ProductsRepository>();
builder.Services.AddScoped<ProductImagesRepository>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// ? Health endpoints (required by your capstone spec)

// LIVE: app is running (always 200)
app.MapGet("/health/live", () =>
{
    return Results.Ok(new { status = "live" });
});

// READY: DB must be reachable (200 if OK, 503 if not)
app.MapGet("/health/ready", async (DapperContext ctx) =>
{
    try
    {
        using var conn = ctx.CreateConnection();
        conn.Open();

        var ok = await conn.ExecuteScalarAsync<int>("SELECT 1;");
        if (ok == 1)
            return Results.Ok(new { status = "ready", db = "ok" });

        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
    catch (Exception ex)
    {
        // Return 503 so Kubernetes removes pod from routing if DB is down
        return Results.Problem(
            title: "Not Ready",
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable
        );
    }
});

app.MapControllers();

app.Run();
