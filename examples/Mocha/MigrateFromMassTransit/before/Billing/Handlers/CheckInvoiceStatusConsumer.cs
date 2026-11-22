using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shop.Contracts;

namespace Shop.Billing;

/// <summary>
/// Responds to <see cref="CheckInvoiceStatus"/> with the current invoice status.
/// </summary>
public sealed class CheckInvoiceStatusConsumer(BillingDbContext db) : IConsumer<CheckInvoiceStatus>
{
    public async Task Consume(ConsumeContext<CheckInvoiceStatus> context)
    {
        // Respond to the request on its reply address.
        var invoice = await db.Invoices.AsNoTracking()
            .SingleOrDefaultAsync(x => x.OrderId == context.Message.OrderId, context.CancellationToken);
        await context.RespondAsync(new InvoiceStatus(context.Message.OrderId, invoice?.Status ?? "NotFound"));
    }
}
