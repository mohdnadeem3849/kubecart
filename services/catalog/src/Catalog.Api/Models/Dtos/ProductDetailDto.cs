namespace Catalog.Api.Models.Dtos;

public class ProductDetailDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public List<string> ImageUrls { get; set; } = new();
}
