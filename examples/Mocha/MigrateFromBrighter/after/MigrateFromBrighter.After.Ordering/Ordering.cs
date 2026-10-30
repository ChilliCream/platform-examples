using Microsoft.EntityFrameworkCore;
using Mocha;
using Mocha.Mediator;
using Mocha.Outbox;
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
            await bus.PublishAsync(new OrderPlaced
            {
                OrderId = command.OrderId,
                Product = command.Product,
                ConfirmInvoice = command.ConfirmInvoice
            }, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });

        await bus.ScheduleSendAsync(new ExpireOrder { OrderId = command.OrderId },
            DateTimeOffset.UtcNow.AddSeconds(configuration.GetValue<int>("Scenario:OrderTimeoutSeconds")), cancellationToken);
        await bus.ScheduleSendAsync(new SendReminder { OrderId = command.OrderId },
            DateTimeOffset.UtcNow.AddSeconds(configuration.GetValue<int>("Scenario:ReminderDelaySeconds")), cancellationToken);
        logger.LogInformation("Order {OrderId} submitted", command.OrderId);
    }
}

public sealed class InvoiceConfirmedHandler(OrderingDataSource db, ILogger<InvoiceConfirmedHandler> logger)
    : IEventHandler<InvoiceConfirmed>
{
    public async ValueTask HandleAsync(InvoiceConfirmed message, CancellationToken cancellationToken)
    {
        await using var command = db.Value.CreateCommand("update order_processes set status = 'Accepted' where order_id = $1 and status = 'Submitted'");
        command.Parameters.AddWithValue(message.OrderId);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 1)
        {
            logger.LogInformation("Order {OrderId} accepted", message.OrderId);
        }
    }
}

public sealed class ExpireOrderHandler(OrderingDataSource db, ILogger<ExpireOrderHandler> logger)
    : IEventRequestHandler<ExpireOrder>
{
    public async ValueTask HandleAsync(ExpireOrder message, CancellationToken cancellationToken)
    {
        await using var command = db.Value.CreateCommand("update order_processes set status = 'Expired' where order_id = $1 and status = 'Submitted'");
        command.Parameters.AddWithValue(message.OrderId);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 1)
        {
            logger.LogInformation("Order {OrderId} expired", message.OrderId);
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
