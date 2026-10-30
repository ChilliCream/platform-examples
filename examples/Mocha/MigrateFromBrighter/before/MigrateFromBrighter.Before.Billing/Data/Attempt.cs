namespace Shop.Billing;

public sealed class Attempt
{
    public Guid OrderId { get; set; }
    public int Count { get; set; }
}
