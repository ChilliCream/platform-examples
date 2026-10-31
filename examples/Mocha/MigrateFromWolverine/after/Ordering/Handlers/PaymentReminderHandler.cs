using Mocha;
using Shop.Contracts;

namespace Shop.Ordering;

/// <summary>
/// Logs a <see cref="PaymentReminder"/> for an order.
/// </summary>
public class PaymentReminderHandler(ILogger<PaymentReminder> logger) : IEventRequestHandler<PaymentReminder>
{
    // Mocha invokes this handler when the scheduled reminder is due.
    public ValueTask HandleAsync(PaymentReminder message, CancellationToken cancellationToken)
    {
        Logs.PaymentReminder(logger, message.OrderId);
        return ValueTask.CompletedTask;
    }
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Payment reminder for order {OrderId}")]
    public static partial void PaymentReminder(ILogger logger, Guid orderId);
}
