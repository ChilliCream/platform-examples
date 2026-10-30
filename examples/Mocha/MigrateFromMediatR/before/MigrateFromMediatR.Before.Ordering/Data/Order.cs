

namespace Shop.Ordering;

public sealed class Order
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; } = "Submitted";
}
