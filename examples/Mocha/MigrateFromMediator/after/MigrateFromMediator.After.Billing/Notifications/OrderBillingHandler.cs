using Mocha.Mediator;
using Microsoft.Extensions.Logging;
using Shop.Ordering;

namespace Shop.Billing;

// Billing handles the same in-process notification in its own assembly.
public sealed class OrderBillingHandler(ILogger<OrderBillingHandler> logger)
    : INotificationHandler<OrderPlacedNotification>
{
    public ValueTask HandleAsync(OrderPlacedNotification notification, CancellationToken cancellationToken)
    {
        Logs.BillingNotified(logger, notification.OrderId);
        return ValueTask.CompletedTask;
    }
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Billing notified for order {OrderId}")]
    internal static partial void BillingNotified(ILogger logger, Guid orderId);
}
