namespace Shop.Ordering;

/// <summary>
/// Records a reminder sent for an order.
/// </summary>
public sealed class Reminder
{
    public Guid OrderId { get; set; }
}
