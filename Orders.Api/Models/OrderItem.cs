namespace Orders.Api.Models;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }

    public string? ProductNameSnapshot { get; set; }
    public decimal UnitPriceSnapshot { get; set; }
    public string? ImageUrlSnapshot { get; set; }

    public DateTime CreatedAtUtc { get; set; } // include only if column exists
}
