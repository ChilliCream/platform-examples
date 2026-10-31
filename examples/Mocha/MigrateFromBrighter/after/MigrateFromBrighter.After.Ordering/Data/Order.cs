namespace Shop.Ordering;

/// <summary>
/// Represents a submitted order and its product.
/// </summary>
public sealed class Order
{
    public Guid Id { get; set; }
    public string Product { get; set; } = string.Empty;
}
