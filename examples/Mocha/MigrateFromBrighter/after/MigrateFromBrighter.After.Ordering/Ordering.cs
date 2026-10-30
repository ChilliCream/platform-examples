using Microsoft.EntityFrameworkCore;
using Mocha;
using Mocha.Mediator;
using Mocha.Outbox;
using Mocha.Sagas;
using Mocha.Sagas.EfCore;
using Mocha.Scheduling;
using Npgsql;
using Shop.Contracts;

namespace Shop.Ordering;

public sealed record PlaceOrder(Guid OrderId, string Product, bool ConfirmInvoice) : ICommand;

public sealed class PlaceOrderHandler(
    OrderingDbContext db,
    IMessageBus bus,
    IConfiguration configuration,
    ILogger<PlaceOrderHandler> logger) : ICommandHandler<PlaceOrder>
{
    public async ValueTask HandleAsync(PlaceOrder command, CancellationToken cancellationToken)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            await db.Database.ExecuteSqlAsync($"""
                insert into orders (id, product) values ({command.OrderId}, {command.Product});
                insert into order_processes (order_id, status) values ({command.OrderId}, 'Submitted');
                """, cancellationToken);
            await bus.SendAsync(new StartOrder
            {
                OrderId = command.OrderId,
                Product = command.Product,
                ConfirmInvoice = command.ConfirmInvoice,
                ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(configuration.GetValue<int>("Scenario:OrderTimeoutSeconds"))
            }, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });

        await bus.ScheduleSendAsync(new SendReminder { OrderId = command.OrderId },
            DateTimeOffset.UtcNow.AddSeconds(configuration.GetValue<int>("Scenario:ReminderDelaySeconds")), cancellationToken);
        logger.LogInformation("Order {OrderId} submitted", command.OrderId);
    }
}

public sealed record StartOrder
{
    public Guid OrderId { get; init; }
    public string Product { get; init; } = string.Empty;
    public bool ConfirmInvoice { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
}

public sealed record ScheduleOrderExpiry(Guid OrderId, DateTimeOffset ExpiresAt);
public sealed record OrderProcessChanged(Guid OrderId, string Status);

public sealed class OrderSagaState : SagaStateBase
{
    public string Product { get; set; } = string.Empty;
    public bool ConfirmInvoice { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}

public sealed class OrderSaga : Saga<OrderSagaState>
{
    protected override void Configure(ISagaDescriptor<OrderSagaState> descriptor)
    {
        descriptor.Initially()
            .OnRequest<StartOrder>()
            .StateFactory(message => new OrderSagaState
            {
                Id = message.OrderId,
                Product = message.Product,
                ConfirmInvoice = message.ConfirmInvoice,
                ExpiresAt = message.ExpiresAt
            })
            .Publish((_, state) => new ScheduleOrderExpiry(state.Id, state.ExpiresAt))
            .Publish((_, state) => new OrderPlaced
            {
                OrderId = state.Id,
                Product = state.Product,
                ConfirmInvoice = state.ConfirmInvoice
            })
            .TransitionTo("Submitted");

        descriptor.During("Submitted")
            .OnEvent<InvoiceConfirmed>()
            .Publish((_, state) => new OrderProcessChanged(state.Id, "Accepted"))
            .TransitionTo("Accepted");

        descriptor.During("Submitted")
            .OnRequest<ExpireOrder>()
            .Publish((_, state) => new OrderProcessChanged(state.Id, "Expired"))
            .TransitionTo("Expired");

        descriptor.Finally("Accepted");
        descriptor.Finally("Expired");
    }
}

public sealed class ScheduleOrderExpiryHandler(IMessageBus bus) : IEventHandler<ScheduleOrderExpiry>
{
    public async ValueTask HandleAsync(ScheduleOrderExpiry message, CancellationToken cancellationToken)
    {
        await bus.ScheduleSendAsync(new ExpireOrder { OrderId = message.OrderId }, message.ExpiresAt, cancellationToken);
    }
}

public sealed class OrderProcessChangedHandler(OrderingDataSource db, ILogger<OrderProcessChangedHandler> logger)
    : IEventHandler<OrderProcessChanged>
{
    public async ValueTask HandleAsync(OrderProcessChanged message, CancellationToken cancellationToken)
    {
        await using var command = db.Value.CreateCommand("update order_processes set status = $1 where order_id = $2 and status = 'Submitted'");
        command.Parameters.AddWithValue(message.Status);
        command.Parameters.AddWithValue(message.OrderId);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 1)
        {
            if (message.Status == "Accepted")
            {
                logger.LogInformation("Order {OrderId} accepted", message.OrderId);
            }
            else
            {
                logger.LogInformation("Order {OrderId} expired", message.OrderId);
            }
        }
    }
}

public sealed class SendReminderHandler(OrderingDataSource db, ILogger<SendReminderHandler> logger)
    : IEventRequestHandler<SendReminder>
{
    public async ValueTask HandleAsync(SendReminder message, CancellationToken cancellationToken)
    {
        await using var command = db.Value.CreateCommand("insert into reminders (order_id) values ($1) on conflict do nothing");
        command.Parameters.AddWithValue(message.OrderId);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 1)
        {
            logger.LogInformation("Reminder sent for order {OrderId}", message.OrderId);
        }
    }
}

public sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddPostgresOutbox();
        modelBuilder.AddPostgresSagas();
        modelBuilder.AddPostgresScheduledMessages();
    }
}

public sealed class OrderingDataSource(NpgsqlDataSource value) : IDisposable
{
    public NpgsqlDataSource Value { get; } = value;
    public void Dispose() => Value.Dispose();
}

public static class OrderingDatabase
{
    public static async Task InitializeAsync(OrderingDataSource db)
    {
        await using var command = db.Value.CreateCommand("""
            create table if not exists orders (
                id uuid primary key,
                product text not null
            );
            create table if not exists order_processes (
                order_id uuid primary key references orders(id),
                status text not null
            );
            create table if not exists reminders (
                order_id uuid primary key references orders(id)
            );
            """);
        await command.ExecuteNonQueryAsync();
    }
}
