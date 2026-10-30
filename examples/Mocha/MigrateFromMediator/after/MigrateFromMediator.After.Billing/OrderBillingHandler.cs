using Mocha.Mediator;
using Microsoft.Extensions.Logging;
using Shop.Ordering;

namespace Shop.Billing;

public sealed class OrderBillingHandler(ILogger<OrderBillingHandler> logger) : INotificationHandler<OrderPlaced>
{
    public ValueTask HandleAsync(OrderPlaced notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Billing notified for order {OrderId}", notification.OrderId);
        return ValueTask.CompletedTask;
    }
}
