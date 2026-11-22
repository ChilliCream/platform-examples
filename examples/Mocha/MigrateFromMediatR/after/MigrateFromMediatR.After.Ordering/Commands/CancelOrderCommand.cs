using Mocha.Mediator;

namespace Shop.Ordering;

/// <summary>
/// Requests cancellation of an existing <see cref="Order"/>.
/// </summary>
/// <param name="OrderId">The ID of the order to cancel.</param>
public sealed record CancelOrderCommand(Guid OrderId) : ICommand;

/// <summary>
/// Cancels an existing order within the command transaction.
/// </summary>
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
