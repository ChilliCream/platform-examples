using Mediator;

namespace Shop.Ordering;

/// <summary>
/// Represents an order command that can be validated.
/// </summary>
public interface IOrderCommand : IMessage
{
    string? Validate();
}
