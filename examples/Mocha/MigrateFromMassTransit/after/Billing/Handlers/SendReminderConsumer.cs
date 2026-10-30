using Mocha;
using Shop.Contracts;

namespace Shop.Billing;

public sealed class SendReminderConsumer(ILogger<SendReminderConsumer> logger) : IEventHandler<SendReminder>
{
    public ValueTask HandleAsync(SendReminder message, CancellationToken cancellationToken)
    {
        // Scheduled reminders arrive as ordinary published messages.
        Logs.ReminderDelivered(logger, message.OrderId);
        return ValueTask.CompletedTask;
    }
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Reminder for order {OrderId} delivered")]
    public static partial void ReminderDelivered(ILogger logger, Guid orderId);
}
