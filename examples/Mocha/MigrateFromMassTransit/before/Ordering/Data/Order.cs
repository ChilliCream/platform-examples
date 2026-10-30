namespace Shop.Ordering;

public sealed class Order
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public bool ConfirmInvoice { get; set; }
}
