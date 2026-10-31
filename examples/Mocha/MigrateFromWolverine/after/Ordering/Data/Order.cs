namespace Shop.Ordering;

/// <summary>
/// Represents an order and its current status.
/// </summary>
public sealed class Order
{
    public Guid Id { get; set; }
    public string Product { get; set; } = "";
    public string Status { get; set; } = "Submitted";
}
