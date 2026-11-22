namespace Shop.Ordering;

/// <summary>
/// Represents a stored product order.
/// </summary>
public sealed class Order
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; } = "Placed";
}
