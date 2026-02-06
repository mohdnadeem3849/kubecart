namespace Catalog.Api.Models.Dtos;

public sealed class UpdateProductRequest
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public string? ImageUrl { get; set; }

    // Optional for update; if not provided, controller can default to 0 or keep old
    public int? Stock { get; set; }
}
