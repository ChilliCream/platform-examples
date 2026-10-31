using Mocha.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Shop.Ordering;

/// <summary>
/// Requests cancellation of an order.
/// </summary>
/// <param name="OrderId">The identifier of the order to cancel.</param>
public sealed record CancelOrderCommand(Guid OrderId) : ICommand, IOrderCommand
{
    public string? Validate()
    {
        if (OrderId == Guid.Empty)
        {
            return "OrderId is required.";
        }

        return null;
    }
}

// The exception processor or middleware observes a missing order before it reaches the API.
/// <summary>
/// Cancels the order identified by a <see cref="CancelOrderCommand"/>.
/// </summary>
public sealed class CancelOrderHandler(IDbContextFactory<OrderDbContext> factory) : ICommandHandler<CancelOrderCommand>
{
    public async ValueTask HandleAsync(CancelOrderCommand message, CancellationToken cancellationToken)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var order = await db.Orders.FindAsync([message.OrderId], cancellationToken);

        if (order is null)
        {
            throw new OrderNotFoundException(message.OrderId);
        }

        order.Status = "Cancelled";
        await db.SaveChangesAsync(cancellationToken);
    }
}
