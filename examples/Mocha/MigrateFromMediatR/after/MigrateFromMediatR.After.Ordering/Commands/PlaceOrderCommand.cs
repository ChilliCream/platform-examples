using Mocha.Mediator;

namespace Shop.Ordering;

public sealed record PlaceOrderCommand(Guid ProductId, int Quantity) : ICommand<Guid>;

// Save the order and return its ID to the caller.
public sealed class PlaceOrderHandler(OrderingDbContext db) : ICommandHandler<PlaceOrderCommand, Guid>
{
    public async ValueTask<Guid> HandleAsync(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            Quantity = request.Quantity
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
