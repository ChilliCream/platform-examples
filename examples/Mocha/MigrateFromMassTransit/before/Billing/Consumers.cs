using System.ComponentModel.DataAnnotations;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shop.Contracts;

namespace Shop.Billing;

public sealed class OrderPlacedConsumer(BillingDbContext db, IConfiguration configuration, ILogger<OrderPlacedConsumer> logger) : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var message = context.Message;
        db.Invoices.Add(new Invoice { Id = message.OrderId, OrderId = message.OrderId, Amount = message.Amount, Status = "Created" });
        await db.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("Invoice for order {OrderId} created", message.OrderId);
        if (message.ConfirmInvoice)
        {
            var delay = configuration.GetValue<int>("Scenario:ConfirmationDelaySeconds");
            await context.SchedulePublish(DateTime.UtcNow.AddSeconds(delay), new InvoiceConfirmed(message.OrderId));
        }
    }
}

public sealed class CheckInvoiceStatusConsumer(BillingDbContext db) : IConsumer<CheckInvoiceStatus>
{
    public async Task Consume(ConsumeContext<CheckInvoiceStatus> context)
    {
        var invoice = await db.Invoices.AsNoTracking().SingleOrDefaultAsync(x => x.OrderId == context.Message.OrderId, context.CancellationToken);
        await context.RespondAsync(new InvoiceStatus(context.Message.OrderId, invoice?.Status ?? "NotFound"));
    }
}

public sealed class ProcessInvoiceConsumer(ILogger<ProcessInvoiceConsumer> logger) : IConsumer<ProcessInvoice>
{
    public Task Consume(ConsumeContext<ProcessInvoice> context)
    {
        var message = context.Message;
        var retry = context.GetRetryAttempt();
        var redelivery = context.GetRedeliveryCount();
        logger.LogInformation("Processing order {OrderId}, failure {Failure}, retry {Retry}, redelivery {Redelivery}", message.OrderId, message.Failure, retry, redelivery);
        if (message.Failure == "validation")
        {
            throw new ValidationException("Invoice is invalid");
        }
        if (message.Failure == "transient" && redelivery == 0)
        {
            throw new InvalidOperationException("Temporary invoice failure");
        }
        logger.LogInformation("Order {OrderId} processing completed", message.OrderId);
        return Task.CompletedTask;
    }
}

public sealed class SendReminderConsumer(ILogger<SendReminderConsumer> logger) : IConsumer<SendReminder>
{
    public Task Consume(ConsumeContext<SendReminder> context)
    {
        logger.LogInformation("Reminder for order {OrderId} delivered", context.Message.OrderId);
        return Task.CompletedTask;
    }
}
