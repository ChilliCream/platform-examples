using Microsoft.EntityFrameworkCore;
using Npgsql;
using Paramore.Brighter;
using Shop.Contracts;

namespace Shop.Ordering;

/// <summary>
/// Stores each order reminder once, including on redelivery.
/// </summary>
public sealed class SendReminderHandler(OrderingDbContext db, ILogger<SendReminderHandler> logger)
    : RequestHandlerAsync<SendReminder>
{
    public override async Task<SendReminder> HandleAsync(
        SendReminder message, CancellationToken cancellationToken = default)
    {
        var reminder = await db.Reminders.FindAsync([message.OrderId], cancellationToken);
        if (reminder is null)
        {
            reminder = new Reminder { OrderId = message.OrderId };
            db.Reminders.Add(reminder);
            try
            {
                await db.SaveChangesAsync(cancellationToken);
                Logs.ReminderSent(logger, message.OrderId);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                // A concurrent delivery already inserted this row.
                db.Entry(reminder).State = EntityState.Detached;
            }
        }

        return await base.HandleAsync(message, cancellationToken);
    }
}

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Information, "Reminder sent for order {OrderId}")]
    public static partial void ReminderSent(ILogger logger, Guid orderId);
}
