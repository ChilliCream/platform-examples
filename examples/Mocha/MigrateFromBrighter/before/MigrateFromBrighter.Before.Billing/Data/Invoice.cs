namespace Shop.Billing;

/// <summary>
/// Represents the billing status of an order.
/// </summary>
public sealed class Invoice
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
}
