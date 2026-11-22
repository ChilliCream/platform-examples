namespace Shop.Ordering;

/// <summary>
/// Represents an order and its invoice confirmation preference.
/// </summary>
public sealed class Order
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public bool ConfirmInvoice { get; set; }
}
