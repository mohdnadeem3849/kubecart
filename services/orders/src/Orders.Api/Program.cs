using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Orders.Api.Clients;
using Orders.Api.Data;
using Orders.Api.Data.Repositories;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ---------------- Swagger + JWT button ----------------
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "KubeCart Orders API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste ONLY the JWT token. Swagger sends: Authorization: Bearer <token>"
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

// ---------------- JWT (must match Identity.Api) ----------------
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
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

// ---------------- DB context ----------------
builder.Services.AddScoped<DapperContext>();

// ---------------- Repositories ----------------
builder.Services.AddScoped<CartRepository>();
builder.Services.AddScoped<CheckoutRepository>();
builder.Services.AddScoped<OrdersRepository>(); // keep only if you actually use it

// ---------------- Catalog Http Client ----------------
var catalogBaseUrl =
    Environment.GetEnvironmentVariable("CATALOG_SERVICE_URL")
    ?? builder.Configuration["ServiceUrls:Catalog"]
    ?? "https://localhost:7221";

builder.Services.AddHttpClient<CatalogClient>(client =>
{
    client.BaseAddress = new Uri(catalogBaseUrl);
});

var app = builder.Build();

// Swagger always
app.UseSwagger();
app.UseSwaggerUI();

app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// ---------------- Health endpoints ----------------
app.MapGet("/health/live", () => Results.Ok(new { status = "live" }))
   .AllowAnonymous();

app.MapGet("/health/ready", async (DapperContext ctx) =>
{
    try
    {
        using var conn = ctx.CreateConnection();
        await conn.ExecuteScalarAsync<int>("SELECT 1;");
        return Results.Ok(new { status = "ready", db = "ok" });
    }
    catch (Exception ex)
    {
        return Results.Problem(title: "DB not ready", detail: ex.Message, statusCode: 503);
    }
}).AllowAnonymous();

// Optional debug endpoint
app.MapGet("/debug/catalog/{id:int}", async (int id, CatalogClient catalog) =>
{
    var p = await catalog.GetProductAsync(id);
    return p is null ? Results.NotFound(new { message = "product not found in catalog" }) : Results.Ok(p);
}).AllowAnonymous();

app.MapControllers();
app.Run();
