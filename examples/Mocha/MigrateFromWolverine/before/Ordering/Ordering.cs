using System.ComponentModel.DataAnnotations;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shop.Contracts;
using Wolverine;
using Wolverine.EntityFrameworkCore;

namespace Shop.Ordering;

public sealed class ScenarioOptions
{
    public int SagaTimeoutSeconds { get; set; } = 30;
    public int ReminderSeconds { get; set; } = 10;
    public int RetryDelaySeconds { get; set; } = 2;
    public int BatchTimeoutSeconds { get; set; } = 2;
}

public sealed class OrderingDb(DbContextOptions<OrderingDb> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderSaga> Sagas => Set<OrderSaga>();
}

public sealed class Order
{
    public Guid Id { get; set; }
    public string Product { get; set; } = "";
    public string Status { get; set; } = "Submitted";
}

public record PlaceOrder(string Product, bool ConfirmInvoice = true);
public record OrderSubmitted(Guid Id);
public record OrderExpired(Guid Id, TimeSpan Delay) : TimeoutMessage(Delay);

public static class PlaceOrderHandler
{
    public static async Task<Guid> Handle(PlaceOrder command, IDbContextOutbox<OrderingDb> outbox, ILogger<PlaceOrder> logger)
    {
        var id = Guid.NewGuid();
        outbox.DbContext.Orders.Add(new Order { Id = id, Product = command.Product });
        await outbox.PublishAsync(new OrderSubmitted(id));
        await outbox.PublishAsync(new OrderPlaced(id, command.Product, command.ConfirmInvoice));
        await outbox.SaveChangesAndFlushMessagesAsync();
        logger.LogInformation("Order {OrderId} submitted", id);
        return id;
    }
}

public class OrderSaga : Saga
{
    public Guid Id { get; set; }

    public static (OrderSaga, OrderExpired) Start(OrderSubmitted message, IOptions<ScenarioOptions> options, ILogger<OrderSaga> logger)
    {
        logger.LogInformation("Order {OrderId} saga submitted", message.Id);
        return (new OrderSaga { Id = message.Id },
            new OrderExpired(message.Id, TimeSpan.FromSeconds(options.Value.SagaTimeoutSeconds)));
    }

    public async Task Handle(InvoiceConfirmed message, OrderingDb db, ILogger<OrderSaga> logger)
    {
        var order = await db.Orders.FindAsync(Id);
        if (order is not null)
        {
            order.Status = "Accepted";
        }
        logger.LogInformation("Order {OrderId} accepted", Id);
        MarkCompleted();
    }

    public async Task Handle(OrderExpired message, OrderingDb db, ILogger<OrderSaga> logger)
    {
        var order = await db.Orders.FindAsync(Id);
        if (order is not null)
        {
            order.Status = "Expired";
        }
        logger.LogInformation("Order {OrderId} expired", Id);
        MarkCompleted();
    }
}

public static class PaymentReminderHandler
{
    public static void Handle(PaymentReminder message, ILogger<PaymentReminder> logger)
        => logger.LogInformation("Payment reminder for order {OrderId}", message.OrderId);
}

public static class AuditOrderHandler
{
    public static void Handle(AuditOrder[] messages, ILogger<AuditOrder> logger)
        => logger.LogInformation("Audited {Count} orders", messages.Length);
}

public static class FailOrderHandler
{
    private static readonly ConcurrentDictionary<Guid, int> Attempts = new();

    public static void Handle(FailOrder message, Envelope envelope, ILogger<FailOrder> logger)
    {
        var attempt = Attempts.AddOrUpdate(envelope.Id, 1, (_, value) => value + 1);
        logger.LogInformation("Processing {Failure} failure for order {OrderId}, attempt {Attempt}", message.Failure, message.OrderId, attempt);
        if (message.Failure == "validation")
        {
            Attempts.TryRemove(envelope.Id, out _);
            throw new ValidationException("Order failed validation");
        }
        if (message.Failure == "transient" && attempt <= 3)
        {
            throw new InvalidOperationException("Temporary order failure");
        }
        Attempts.TryRemove(envelope.Id, out _);
    }
}
