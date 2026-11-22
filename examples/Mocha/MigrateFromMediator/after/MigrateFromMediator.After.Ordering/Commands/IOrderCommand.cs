namespace Shop.Ordering;

/// <summary>
/// Represents an order command that can be validated.
/// </summary>
public interface IOrderCommand
{
    string? Validate();
}
