namespace Catalog.Api.Models.Dtos;

public sealed class CreateProductRequest
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public string? ImageUrl { get; set; }

    // DB column is NOT NULL -> we always want a value
    public int Stock { get; set; } = 0;
}
