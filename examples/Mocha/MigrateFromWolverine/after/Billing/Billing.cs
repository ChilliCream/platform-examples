using Microsoft.EntityFrameworkCore;
using Mocha;
using Mocha.Inbox;
using Mocha.Outbox;
using Shop.Contracts;

namespace Shop.Billing;

public sealed class BillingDb(DbContextOptions<BillingDb> options) : DbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>().HasKey(x => x.OrderId);
        modelBuilder.AddPostgresOutbox();
        modelBuilder.AddPostgresInbox();
    }
}

public sealed class Invoice
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = "Created";
}

public class OrderPlacedHandler(BillingDb db, IMessageBus bus, ILogger<OrderPlaced> logger) : IEventHandler<OrderPlaced>
{
    public async ValueTask HandleAsync(OrderPlaced message, CancellationToken cancellationToken)
    {
        db.Invoices.Add(new Invoice { OrderId = message.OrderId });
        logger.LogInformation("Invoice created for order {OrderId}", message.OrderId);
        if (message.ConfirmInvoice)
        {
            await bus.PublishAsync(new InvoiceConfirmed(message.OrderId), cancellationToken);
        }
        await db.SaveChangesAsync(cancellationToken);
    }
}

public class CheckInvoiceStatusHandler(BillingDb db) : IEventRequestHandler<CheckInvoiceStatus, InvoiceStatus>
{
    public async ValueTask<InvoiceStatus> HandleAsync(CheckInvoiceStatus message, CancellationToken cancellationToken)
    {
        var invoice = await db.Invoices.FindAsync([message.OrderId], cancellationToken);
        return new InvoiceStatus(message.OrderId, invoice?.Status ?? "NotFound");
    }
}
