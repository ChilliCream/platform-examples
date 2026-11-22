using Microsoft.EntityFrameworkCore;
using Mocha;
using Mocha.Mediator;
using Shop.Contracts;

namespace Shop.Ordering;

/// <summary>
/// Stores the terminal status of an order after its completed saga state is removed.
/// </summary>
public sealed class OrderProcessChangedHandler(OrderingDbContext db, ILogger<OrderProcessChangedHandler> logger)
    : IEventHandler<OrderProcessChanged>
{
    public async ValueTask HandleAsync(OrderProcessChanged message, CancellationToken cancellationToken)
    {
        var updated = await db.OrderProcesses
            .Where(x => x.OrderId == message.OrderId && x.Status == "Submitted")
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.Status, message.Status), cancellationToken);
        if (updated == 1)
        {
            if (message.Status == "Accepted")
            {
                Logs.OrderAccepted(logger, message.OrderId);
            }
            else
            {
                Logs.OrderExpired(logger, message.OrderId);
            }
        }
    }
}

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Information, "Order {OrderId} accepted")]
    public static partial void OrderAccepted(ILogger logger, Guid orderId);

    [LoggerMessage(LogLevel.Information, "Order {OrderId} expired")]
    public static partial void OrderExpired(ILogger logger, Guid orderId);
}
