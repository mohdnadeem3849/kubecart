namespace Orders.Api.Models;

public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Notes { get; set; } = "";
    public string Status { get; set; } = "Pending";
    public decimal TotalAmount { get; set; } // ✅ ADD THIS
    public DateTime CreatedAtUtc { get; set; }
}
