using Mocha.Mediator;

namespace Shop.Ordering;

public sealed record CancelOrderCommand(Guid OrderId) : ICommand;

// Cancel an existing order within the command transaction.
public sealed class CancelOrderHandler(OrderingDbContext db) : ICommandHandler<CancelOrderCommand>
{
    public async ValueTask HandleAsync(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await db.Orders.FindAsync([request.OrderId], cancellationToken);

        if (order is null)
        {
            throw new OrderNotFoundException(request.OrderId);
        }

        order.Status = "Cancelled";
        await db.SaveChangesAsync(cancellationToken);
    }
}
