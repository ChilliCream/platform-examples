using Mediator;
using Microsoft.Extensions.Logging;

namespace Shop.Ordering;

/// <summary>
/// Notifies handlers that an order was placed.
/// </summary>
/// <param name="OrderId">The identifier of the order.</param>
/// <param name="ProductId">The identifier of the ordered product.</param>
/// <param name="Quantity">The number of units ordered.</param>
public sealed record OrderPlacedNotification(Guid OrderId, Guid ProductId, int Quantity) : INotification;

/// <summary>
/// Logs an audit entry for an <see cref="OrderPlacedNotification"/>.
/// Both Ordering and Billing handle the notification.
/// </summary>
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
