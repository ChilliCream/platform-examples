namespace Shop.Billing;

public sealed class Invoice
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = "Created";
}
