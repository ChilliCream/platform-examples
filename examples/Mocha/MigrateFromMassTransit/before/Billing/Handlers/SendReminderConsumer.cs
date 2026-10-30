using MassTransit;
using Shop.Contracts;

namespace Shop.Billing;

public sealed class SendReminderConsumer(ILogger<SendReminderConsumer> logger) : IConsumer<SendReminder>
{
    public Task Consume(ConsumeContext<SendReminder> context)
    {
        // Scheduled reminders arrive as ordinary published messages.
        Logs.ReminderDelivered(logger, context.Message.OrderId);
        return Task.CompletedTask;
    }
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Reminder for order {OrderId} delivered")]
    public static partial void ReminderDelivered(ILogger logger, Guid orderId);
}
