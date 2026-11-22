namespace Shop.Billing;

/// <summary>
/// Represents the invoice and its status for an order.
/// </summary>
public sealed class Invoice
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = "Created";
}
