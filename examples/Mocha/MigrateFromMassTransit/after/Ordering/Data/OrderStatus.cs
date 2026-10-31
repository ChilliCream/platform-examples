namespace Shop.Ordering;

public sealed class OrderStatus
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
}
