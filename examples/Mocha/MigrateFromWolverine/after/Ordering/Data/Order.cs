namespace Shop.Ordering;

public sealed class Order
{
    public Guid Id { get; set; }
    public string Product { get; set; } = "";
    public string Status { get; set; } = "Submitted";
}
