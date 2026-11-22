namespace Shop.Billing;

/// <summary>
/// Records the number of simulated billing attempts for an order.
/// </summary>
public sealed class Attempt
{
    public Guid OrderId { get; set; }
    public int Count { get; set; }
}
