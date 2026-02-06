namespace Catalog.Api.Models.Dtos;

public class ProductSummaryDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; } // first image (optional)
}
