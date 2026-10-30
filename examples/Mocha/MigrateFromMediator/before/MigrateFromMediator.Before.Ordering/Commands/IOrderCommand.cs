using Mediator;

namespace Shop.Ordering;

public interface IOrderCommand : IMessage
{
    string? Validate();
}
