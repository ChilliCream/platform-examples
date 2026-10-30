using MediatR;

namespace Shop.Ordering;

public sealed record PlaceOrderCommand(Guid ProductId, int Quantity) : IRequest<Guid>, IOrderCommand;

// Save the order and return its ID to the caller.
public sealed class PlaceOrderHandler(OrderingDbContext db) : IRequestHandler<PlaceOrderCommand, Guid>
{
    public async Task<Guid> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
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
