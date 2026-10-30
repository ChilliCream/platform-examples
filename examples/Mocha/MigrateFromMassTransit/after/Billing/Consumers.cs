using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Mocha;
using Mocha.Features;
using Shop.Contracts;

namespace Shop.Billing;

public sealed class OrderPlacedConsumer(BillingDbContext db, IMessageBus bus, IConfiguration configuration, ILogger<OrderPlacedConsumer> logger) : IEventHandler<OrderPlaced>
{
    public async ValueTask HandleAsync(OrderPlaced message, CancellationToken cancellationToken)
    {
        db.Invoices.Add(new Invoice { Id = message.OrderId, OrderId = message.OrderId, Amount = message.Amount, Status = "Created" });
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Invoice for order {OrderId} created", message.OrderId);
        if (message.ConfirmInvoice)
        {
            var delay = configuration.GetValue<int>("Scenario:ConfirmationDelaySeconds");
            await bus.SchedulePublishAsync(new InvoiceConfirmed(message.OrderId), DateTimeOffset.UtcNow.AddSeconds(delay), cancellationToken);
        }
    }
}

public sealed class CheckInvoiceStatusConsumer(BillingDbContext db) : IEventRequestHandler<CheckInvoiceStatus, InvoiceStatus>
{
    public async ValueTask<InvoiceStatus> HandleAsync(CheckInvoiceStatus message, CancellationToken cancellationToken)
    {
        var invoice = await db.Invoices.AsNoTracking().SingleOrDefaultAsync(x => x.OrderId == message.OrderId, cancellationToken);
        return new InvoiceStatus(message.OrderId, invoice?.Status ?? "NotFound");
    }
}

public sealed class ProcessInvoiceConsumer(ConsumeContextAccessor accessor, ILogger<ProcessInvoiceConsumer> logger) : IEventRequestHandler<ProcessInvoice>
{
    public ValueTask HandleAsync(ProcessInvoice message, CancellationToken cancellationToken)
    {
        var retryFeature = accessor.Context?.Features.Get<RetryFeature>();
        var retry = retryFeature?.ImmediateRetryCount ?? 0;
        var redelivery = retryFeature?.DelayedRetryCount ?? 0;
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
        return ValueTask.CompletedTask;
    }
}

public sealed class SendReminderConsumer(ILogger<SendReminderConsumer> logger) : IEventHandler<SendReminder>
{
    public ValueTask HandleAsync(SendReminder message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Reminder for order {OrderId} delivered", message.OrderId);
        return ValueTask.CompletedTask;
    }
}
