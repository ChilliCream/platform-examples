using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Mocha;
using Mocha.Inbox;
using Mocha.Mediator;
using Mocha.Outbox;
using Mocha.Sagas;
using Mocha.Sagas.EfCore;
using Mocha.Scheduling;
using Shop.Contracts;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddPostgresOutbox();
        modelBuilder.AddPostgresInbox();
        modelBuilder.AddPostgresSagas();
        modelBuilder.AddPostgresScheduledMessages();
    }
}

public sealed class Order
{
    public Guid Id { get; set; }
    public string Product { get; set; } = "";
    public string Status { get; set; } = "Submitted";
}

public record PlaceOrder(string Product, bool ConfirmInvoice = true) : ICommand<Guid>;
public record OrderSubmitted(Guid Id);
public record UpdateOrderStatus(Guid Id, string Status);

public class PlaceOrderHandler(OrderingDb db, IMessageBus bus, ILogger<PlaceOrder> logger, ILogger<OrderSaga> sagaLogger)
    : ICommandHandler<PlaceOrder, Guid>
{
    public async ValueTask<Guid> HandleAsync(PlaceOrder command, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        db.Orders.Add(new Order { Id = id, Product = command.Product });
        await bus.PublishAsync(new OrderSubmitted(id), cancellationToken);
        await bus.PublishAsync(new OrderPlaced(id, command.Product, command.ConfirmInvoice), cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Order {OrderId} submitted", id);
        sagaLogger.LogInformation("Order {OrderId} saga submitted", id);
        return id;
    }
}

public class OrderState : SagaStateBase;

public class OrderSaga : Saga<OrderState>
{
    public static TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    protected override void Configure(ISagaDescriptor<OrderState> descriptor)
    {
        descriptor.Timeout(Timeout);
        descriptor.Initially().OnEvent<OrderSubmitted>()
            .StateFactory(message => new OrderState { Id = message.Id })
            .TransitionTo("Submitted");
        descriptor.During("Submitted").OnEvent<InvoiceConfirmed>()
            .Send((_, state) => new UpdateOrderStatus(state.Id, "Accepted"))
            .TransitionTo("Accepted");
        descriptor.During("Submitted").OnTimeout()
            .Send((_, state) => new UpdateOrderStatus(state.Id, "Expired"))
            .TransitionTo(StateNames.TimedOut);
        descriptor.Finally("Accepted");
    }
}

public class UpdateOrderStatusHandler(OrderingDb db, ILogger<OrderSaga> logger) : IEventRequestHandler<UpdateOrderStatus>
{
    public async ValueTask HandleAsync(UpdateOrderStatus message, CancellationToken cancellationToken)
    {
        var order = await db.Orders.FindAsync([message.Id], cancellationToken);
        if (order is not null)
        {
            order.Status = message.Status;
        }
        if (message.Status == "Accepted")
        {
            logger.LogInformation("Order {OrderId} accepted", message.Id);
        }
        else
        {
            logger.LogInformation("Order {OrderId} expired", message.Id);
        }
        await db.SaveChangesAsync(cancellationToken);
    }
}

public class PaymentReminderHandler(ILogger<PaymentReminder> logger) : IEventRequestHandler<PaymentReminder>
{
    public ValueTask HandleAsync(PaymentReminder message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Payment reminder for order {OrderId}", message.OrderId);
        return ValueTask.CompletedTask;
    }
}

public class AuditOrderHandler(ILogger<AuditOrder> logger) : IBatchEventHandler<AuditOrder>
{
    public ValueTask HandleAsync(IMessageBatch<AuditOrder> messages, CancellationToken cancellationToken)
    {
        logger.LogInformation("Audited {Count} orders", messages.Count);
        return ValueTask.CompletedTask;
    }
}

public class FailOrderHandler(ILogger<FailOrder> logger) : IConsumer<FailOrder>
{
    private static readonly ConcurrentDictionary<string, int> Attempts = new();

    public ValueTask ConsumeAsync(IConsumeContext<FailOrder> context)
    {
        var message = context.Message;
        var id = context.MessageId!;
        var attempt = Attempts.AddOrUpdate(id, 1, (_, value) => value + 1);
        logger.LogInformation("Processing {Failure} failure for order {OrderId}, attempt {Attempt}", message.Failure, message.OrderId, attempt);
        if (message.Failure == "validation")
        {
            Attempts.TryRemove(id, out _);
            throw new ValidationException("Order failed validation");
        }
        if (message.Failure == "transient" && attempt <= 3)
        {
            throw new InvalidOperationException("Temporary order failure");
        }
        Attempts.TryRemove(id, out _);
        return ValueTask.CompletedTask;
    }
}
