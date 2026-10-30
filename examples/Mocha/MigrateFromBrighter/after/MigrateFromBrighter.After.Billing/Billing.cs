using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Mocha;
using Mocha.Scheduling;
using Npgsql;
using Shop.Contracts;

namespace Shop.Billing;

public sealed class OrderPlacedHandler(NpgsqlDataSource db, IMessageBus bus, ILogger<OrderPlacedHandler> logger)
    : IEventHandler<OrderPlaced>
{
    public async ValueTask HandleAsync(OrderPlaced message, CancellationToken cancellationToken)
    {
        await using var command = db.CreateCommand("insert into invoices (order_id, status) values ($1, 'Created') on conflict do nothing");
        command.Parameters.AddWithValue(message.OrderId);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 1)
        {
            logger.LogInformation("Invoice created for order {OrderId}", message.OrderId);
        }
        if (message.ConfirmInvoice)
        {
            await bus.PublishAsync(new InvoiceConfirmed { OrderId = message.OrderId }, cancellationToken);
        }
    }
}

public sealed class CheckInvoiceStatusHandler(NpgsqlDataSource db)
    : IEventRequestHandler<CheckInvoiceStatus, InvoiceStatus>
{
    public async ValueTask<InvoiceStatus> HandleAsync(CheckInvoiceStatus message, CancellationToken cancellationToken)
    {
        await using var command = db.CreateCommand("select status from invoices where order_id = $1");
        command.Parameters.AddWithValue(message.OrderId);
        var status = (string?)await command.ExecuteScalarAsync(cancellationToken) ?? "NotFound";
        return new InvoiceStatus { OrderId = message.OrderId, Status = status };
    }
}

public sealed class TransientException(string message) : Exception(message);

public sealed class ExerciseTransientFailureHandler(NpgsqlDataSource db, ILogger<ExerciseTransientFailureHandler> logger)
    : IEventRequestHandler<ExerciseTransientFailure>
{
    public async ValueTask HandleAsync(ExerciseTransientFailure message, CancellationToken cancellationToken)
    {
        await using var command = db.CreateCommand("""
            insert into attempts (order_id, count) values ($1, 1)
            on conflict (order_id) do update set count = attempts.count + 1
            returning count
            """);
        command.Parameters.AddWithValue(message.OrderId);
        var attempt = (int)(await command.ExecuteScalarAsync(cancellationToken))!;
        logger.LogInformation("Transient attempt {Attempt} for order {OrderId}", attempt, message.OrderId);
        if (attempt < 3)
        {
            throw new TransientException("Temporary billing failure");
        }
    }
}

public sealed class ExerciseValidationFailureHandler(ILogger<ExerciseValidationFailureHandler> logger)
    : IEventRequestHandler<ExerciseValidationFailure>
{
    public ValueTask HandleAsync(ExerciseValidationFailure message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Validation failed for order {OrderId}", message.OrderId);
        throw new ValidationException("Invalid billing request");
    }
}

public sealed class BillingDbContext(DbContextOptions<BillingDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddPostgresScheduledMessages();
    }
}

public static class BillingDatabase
{
    public static async Task InitializeAsync(NpgsqlDataSource db)
    {
        await using var command = db.CreateCommand("""
            create table if not exists invoices (
                order_id uuid primary key,
                status text not null
            );
            create table if not exists attempts (
                order_id uuid primary key,
                count integer not null
            );
            """);
        await command.ExecuteNonQueryAsync();
    }
}
