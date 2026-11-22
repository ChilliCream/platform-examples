using Mocha.Mediator;

namespace Shop.Ordering;

/// <summary>
/// Requests an order for a product and returns its ID.
/// </summary>
/// <param name="ProductId">The ID of the product to order.</param>
/// <param name="Quantity">The number of units to order.</param>
public sealed record PlaceOrderCommand(Guid ProductId, int Quantity) : ICommand<Guid>;

/// <summary>
/// Saves an order and returns its ID.
/// </summary>
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
