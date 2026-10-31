using Microsoft.EntityFrameworkCore;
using Paramore.Brighter;
using Shop.Contracts;

namespace Shop.Ordering;

// Brighter has no saga abstraction, so this handler updates the order process directly.
/// <summary>
/// Updates a submitted order process to accepted.
/// </summary>
public sealed class InvoiceConfirmedHandler(OrderingDbContext db, ILogger<InvoiceConfirmedHandler> logger)
    : RequestHandlerAsync<InvoiceConfirmed>
{
    public override async Task<InvoiceConfirmed> HandleAsync(
        InvoiceConfirmed message, CancellationToken cancellationToken = default)
    {
        var updated = await db.OrderProcesses
            .Where(x => x.OrderId == message.OrderId && x.Status == "Submitted")
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.Status, "Accepted"), cancellationToken);
        if (updated == 1)
        {
            Logs.OrderAccepted(logger, message.OrderId);
        }

        return await base.HandleAsync(message, cancellationToken);
    }
}

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Information, "Order {OrderId} accepted")]
    public static partial void OrderAccepted(ILogger logger, Guid orderId);
}
