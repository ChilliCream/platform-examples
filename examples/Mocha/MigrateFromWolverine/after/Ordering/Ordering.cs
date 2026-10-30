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
public record OrderSubmitted(Guid Id, string Product, bool ConfirmInvoice);

public class PlaceOrderHandler(OrderingDb db, IMessageBus bus, ILogger<PlaceOrder> logger)
    : ICommandHandler<PlaceOrder, Guid>
{
    public async ValueTask<Guid> HandleAsync(PlaceOrder command, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        db.Orders.Add(new Order { Id = id, Product = command.Product });
        await bus.PublishAsync(new OrderSubmitted(id, command.Product, command.ConfirmInvoice), cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Order {OrderId} submitted", id);
        return id;
    }
}

public class OrderState : SagaStateBase
{
    public string Product { get; set; } = "";
    public bool ConfirmInvoice { get; set; }
}

public class OrderSaga : Saga<OrderState>
{
    public static TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    protected override void Configure(ISagaDescriptor<OrderState> descriptor)
    {
        descriptor.Timeout(Timeout);
        descriptor.Initially().OnEvent<OrderSubmitted>()
            .StateFactory(message => new OrderState
            {
                Id = message.Id,
                Product = message.Product,
                ConfirmInvoice = message.ConfirmInvoice
            })
            .TransitionTo("Submitted")
            .Publish((_, state) => new OrderPlaced(state.Id, state.Product, state.ConfirmInvoice));
        descriptor.During("Submitted").OnEvent<InvoiceConfirmed>()
            .TransitionTo("Accepted");
        descriptor.During("Submitted").OnTimeout()
            .TransitionTo(StateNames.TimedOut);
        descriptor.Finally("Accepted");
    }

    protected override async Task OnEnterStateAsync(OrderState state, Mocha.Sagas.SagaState nextState, IConsumeContext context)
    {
        var logger = context.Services.GetRequiredService<ILogger<OrderSaga>>();
        if (nextState.State == "Submitted")
        {
            logger.LogInformation("Order {OrderId} saga submitted", state.Id);
        }
        else if (nextState.State is "Accepted" or StateNames.TimedOut)
        {
            var db = context.Services.GetRequiredService<OrderingDb>();
            var order = await db.Orders.FindAsync([state.Id], context.CancellationToken);
            var status = nextState.State == "Accepted" ? "Accepted" : "Expired";
            if (order is not null)
            {
                order.Status = status;
            }
            if (status == "Accepted")
            {
                logger.LogInformation("Order {OrderId} accepted", state.Id);
            }
            else
            {
                logger.LogInformation("Order {OrderId} expired", state.Id);
            }
            await db.SaveChangesAsync(context.CancellationToken);
        }

        await base.OnEnterStateAsync(state, nextState, context);
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
