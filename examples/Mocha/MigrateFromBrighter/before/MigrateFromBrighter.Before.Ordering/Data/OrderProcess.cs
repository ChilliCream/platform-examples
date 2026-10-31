namespace Shop.Ordering;

/// <summary>
/// Stores the current processing status of an order.
/// </summary>
public sealed class OrderProcess
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
}
