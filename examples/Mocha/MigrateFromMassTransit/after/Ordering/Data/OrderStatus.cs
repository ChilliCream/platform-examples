namespace Shop.Ordering;

/// <summary>
/// Stores the visible status of an order saga after it completes.
/// </summary>
public sealed class OrderStatus
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
}
