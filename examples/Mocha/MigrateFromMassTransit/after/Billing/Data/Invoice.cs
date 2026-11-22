namespace Shop.Billing;

/// <summary>
/// Represents an invoice for an order.
/// </summary>
public sealed class Invoice
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = null!;
}
