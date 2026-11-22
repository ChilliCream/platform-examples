using Microsoft.EntityFrameworkCore;
using Paramore.Brighter;
using Shop.Contracts;

namespace Shop.Ordering;

// Brighter has no saga abstraction, so this handler updates the order process directly.
/// <summary>
/// Updates a submitted order process to expired.
/// </summary>
public sealed class ExpireOrderHandler(OrderingDbContext db, ILogger<ExpireOrderHandler> logger)
    : RequestHandlerAsync<ExpireOrder>
{
    public override async Task<ExpireOrder> HandleAsync(
        ExpireOrder message, CancellationToken cancellationToken = default)
    {
        var updated = await db.OrderProcesses
            .Where(x => x.OrderId == message.OrderId && x.Status == "Submitted")
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.Status, "Expired"), cancellationToken);
        if (updated == 1)
        {
            Logs.OrderExpired(logger, message.OrderId);
        }

        return await base.HandleAsync(message, cancellationToken);
    }
}

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Information, "Order {OrderId} expired")]
    public static partial void OrderExpired(ILogger logger, Guid orderId);
}
