namespace Shop.Ordering;

public sealed class OrderProcess
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
}
