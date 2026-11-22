using Microsoft.EntityFrameworkCore;
using Mocha;
using Shop.Contracts;

namespace Shop.Billing;

public class OrderPlacedHandler(
    BillingDbContext db,
    IMessageBus bus,
    IConfiguration configuration,
    ILogger<OrderPlacedHandler> logger) : IEventHandler<OrderPlaced>
{
    public async ValueTask HandleAsync(OrderPlaced message, CancellationToken cancellationToken)
    {
        db.Invoices.Add(new Invoice { OrderId = message.OrderId, Status = "Created" });
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Invoice created for order {OrderId}", message.OrderId);
        if (message.ConfirmInvoice)
        {
            await bus.ScheduleSendAsync(
                new InvoiceConfirmed(message.OrderId),
                DateTimeOffset.UtcNow.AddSeconds(configuration.GetValue<int>("Scenario:ConfirmationDelaySeconds")),
                cancellationToken);
        }
    }
}

public class CheckInvoiceStatusHandler(BillingDbContext db)
    : IEventRequestHandler<CheckInvoiceStatus, InvoiceStatus>
{
    public async ValueTask<InvoiceStatus> HandleAsync(CheckInvoiceStatus message, CancellationToken cancellationToken)
    {
        var status = await db.Invoices
            .Where(x => x.OrderId == message.OrderId)
            .Select(x => x.Status)
            .SingleOrDefaultAsync(cancellationToken) ?? "NotFound";
        return new InvoiceStatus(message.OrderId, status);
    }
}
