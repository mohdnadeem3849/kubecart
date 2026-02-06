namespace Orders.Api.Models;

public class CartItem
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
