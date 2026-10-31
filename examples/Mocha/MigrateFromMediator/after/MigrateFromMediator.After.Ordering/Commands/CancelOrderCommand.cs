using Mocha.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Shop.Ordering;

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
