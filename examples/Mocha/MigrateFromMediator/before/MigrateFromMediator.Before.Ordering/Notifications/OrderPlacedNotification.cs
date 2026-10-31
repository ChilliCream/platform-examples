using Mediator;
using Microsoft.Extensions.Logging;

namespace Shop.Ordering;

public sealed record OrderPlacedNotification(Guid OrderId, Guid ProductId, int Quantity) : INotification;

// Both Ordering and Billing handle this in-process notification.
public sealed class OrderAuditHandler(ILogger<OrderAuditHandler> logger) : INotificationHandler<OrderPlacedNotification>
{
    public ValueTask Handle(OrderPlacedNotification notification, CancellationToken cancellationToken)
    {
        Logs.OrderAuditRecorded(logger, notification.OrderId);
        return ValueTask.CompletedTask;
    }
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Order audit recorded for {OrderId}")]
    internal static partial void OrderAuditRecorded(ILogger logger, Guid orderId);
}
