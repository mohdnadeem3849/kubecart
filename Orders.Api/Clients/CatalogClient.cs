using System.Net;
using System.Net.Http.Json;

namespace Orders.Api.Clients;

public sealed class CatalogClient(HttpClient http)
{
    public sealed class CatalogProduct
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
    }

    public async Task<CatalogProduct?> GetProductAsync(int id)
    {
        var res = await http.GetAsync($"/api/catalog/products/{id}");
        if (res.StatusCode == HttpStatusCode.NotFound) return null;
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<CatalogProduct>();
    }
}
