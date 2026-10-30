using Mocha.Mediator;

namespace Shop.Ordering;

public interface IOrderCommand
{
    string? Validate();
}

public sealed record PlaceOrder(Guid ProductId, int Quantity) : ICommand<Guid>, IOrderCommand
{
    public string? Validate()
        => ProductId == Guid.Empty ? "ProductId is required." : Quantity <= 0 ? "Quantity must be positive." : null;
}

public sealed record CancelOrder(Guid OrderId) : ICommand, IOrderCommand
{
    public string? Validate() => OrderId == Guid.Empty ? "OrderId is required." : null;
}

public sealed record GetOrder(Guid OrderId) : IQuery<OrderDto?>;

public sealed record OrderDto(Guid OrderId, Guid ProductId, int Quantity, string Status);

public sealed record OrderPlaced(Guid OrderId, Guid ProductId, int Quantity) : INotification;
