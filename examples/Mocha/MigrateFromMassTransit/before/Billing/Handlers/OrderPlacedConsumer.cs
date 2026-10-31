using MassTransit;
using Shop.Contracts;

namespace Shop.Billing;

public sealed class OrderPlacedConsumer(
    BillingDbContext db,
    IConfiguration configuration,
    ILogger<OrderPlacedConsumer> logger) : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var message = context.Message;
        db.Invoices.Add(new Invoice
        {
            Id = message.OrderId,
            OrderId = message.OrderId,
            Amount = message.Amount,
            Status = "Created"
        });
        await db.SaveChangesAsync(context.CancellationToken);
        Logs.InvoiceCreated(logger, message.OrderId);

        // Publish confirmation after a short delay so the saga can observe it.
        if (message.ConfirmInvoice)
        {
            var delay = configuration.GetValue<int>("Scenario:ConfirmationDelaySeconds");
            await context.SchedulePublish(DateTime.UtcNow.AddSeconds(delay), new InvoiceConfirmed(message.OrderId));
        }
    }
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Invoice for order {OrderId} created")]
    public static partial void InvoiceCreated(ILogger logger, Guid orderId);
}
