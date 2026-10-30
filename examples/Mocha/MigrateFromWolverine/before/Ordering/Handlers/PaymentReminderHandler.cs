using Shop.Contracts;

namespace Shop.Ordering;

public static class PaymentReminderHandler
{
    // Wolverine invokes this handler when the scheduled reminder is due.
    public static void Handle(PaymentReminder message, ILogger<PaymentReminder> logger)
    {
        Logs.PaymentReminder(logger, message.OrderId);
    }
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Payment reminder for order {OrderId}")]
    public static partial void PaymentReminder(ILogger logger, Guid orderId);
}
