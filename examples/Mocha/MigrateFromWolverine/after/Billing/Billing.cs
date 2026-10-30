using Microsoft.EntityFrameworkCore;
using Shop.Contracts;

namespace Shop.Billing;

public sealed class BillingDb(DbContextOptions<BillingDb> options) : DbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Invoice>().HasKey(x => x.OrderId);
}

public sealed class Invoice
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = "Created";
}

public static class OrderPlacedHandler
{
    public static InvoiceConfirmed? Handle(OrderPlaced message, BillingDb db, ILogger<OrderPlaced> logger)
    {
        db.Invoices.Add(new Invoice { OrderId = message.OrderId });
        logger.LogInformation("Invoice created for order {OrderId}", message.OrderId);
        return message.ConfirmInvoice ? new InvoiceConfirmed(message.OrderId) : null;
    }
}

public static class CheckInvoiceStatusHandler
{
    public static async Task<InvoiceStatus> Handle(CheckInvoiceStatus message, BillingDb db)
    {
        var invoice = await db.Invoices.FindAsync(message.OrderId);
        return new InvoiceStatus(message.OrderId, invoice?.Status ?? "NotFound");
    }
}
