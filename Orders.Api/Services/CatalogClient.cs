using System.Net.Http.Json;

namespace Orders.Api.Services;

public sealed class CatalogClient
{
    private readonly HttpClient _http;

    public CatalogClient(HttpClient http)
    {
        _http = http;
    }

    // Used to validate product exists + get snapshot fields for order items
    public async Task<CatalogProduct?> GetProductByIdAsync(int productId, CancellationToken ct = default)
    {
        // Catalog endpoint: /api/catalog/products/{id}
        var res = await _http.GetAsync($"/api/catalog/products/{productId}", ct);
        if (res.StatusCode == System.Net.HttpStatusCode.NotFound) return null;

        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<CatalogProduct>(cancellationToken: ct);
    }
}

// Matches what your Catalog API returns
public sealed class CatalogProduct
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
