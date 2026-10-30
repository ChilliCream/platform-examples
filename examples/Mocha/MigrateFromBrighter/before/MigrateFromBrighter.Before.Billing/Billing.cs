using System.ComponentModel.DataAnnotations;
using Npgsql;
using Paramore.Brighter;
using Paramore.Brighter.Defer.Attributes;
using Paramore.Brighter.Policies.Attributes;
using Shop.Contracts;

namespace Shop.Billing;

public sealed class OrderPlacedHandler(NpgsqlDataSource db, IAmACommandProcessor processor, ILogger<OrderPlacedHandler> logger)
    : RequestHandlerAsync<OrderPlaced>
{
    public override async Task<OrderPlaced> HandleAsync(OrderPlaced message, CancellationToken cancellationToken = default)
    {
        await using var command = db.CreateCommand("insert into invoices (order_id, status) values ($1, 'Created') on conflict do nothing");
        command.Parameters.AddWithValue(message.OrderId);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 1)
        {
            logger.LogInformation("Invoice created for order {OrderId}", message.OrderId);
        }
        if (message.ConfirmInvoice)
        {
            await processor.PostAsync(new InvoiceConfirmed { OrderId = message.OrderId }, cancellationToken: cancellationToken);
        }
        return await base.HandleAsync(message, cancellationToken);
    }
}

public sealed class CheckInvoiceStatusHandler(NpgsqlDataSource db, IAmACommandProcessor processor)
    : RequestHandlerAsync<CheckInvoiceStatus>
{
    public override async Task<CheckInvoiceStatus> HandleAsync(CheckInvoiceStatus message, CancellationToken cancellationToken = default)
    {
        await using var command = db.CreateCommand("select status from invoices where order_id = $1");
        command.Parameters.AddWithValue(message.OrderId);
        var status = (string?)await command.ExecuteScalarAsync(cancellationToken) ?? "NotFound";
        await processor.PostAsync(new InvoiceStatus(message.ReplyAddress) { OrderId = message.OrderId, Status = status }, cancellationToken: cancellationToken);
        return await base.HandleAsync(message, cancellationToken);
    }
}

public sealed class TransientException(string message) : Exception(message);

public sealed class ExerciseTransientFailureHandler(NpgsqlDataSource db, ILogger<ExerciseTransientFailureHandler> logger)
    : RequestHandlerAsync<ExerciseTransientFailure>
{
    [DeferMessageOnErrorAsync(step: 0, delayMilliseconds: 1000)]
    [UseResiliencePipelineAsync("TransientRetry", step: 1)]
    public override async Task<ExerciseTransientFailure> HandleAsync(ExerciseTransientFailure message, CancellationToken cancellationToken = default)
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
        return await base.HandleAsync(message, cancellationToken);
    }
}

public sealed class ExerciseValidationFailureHandler(ILogger<ExerciseValidationFailureHandler> logger)
    : RequestHandlerAsync<ExerciseValidationFailure>
{
    public override Task<ExerciseValidationFailure> HandleAsync(ExerciseValidationFailure message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Validation failed for order {OrderId}", message.OrderId);
        throw new ValidationException("Invalid billing request");
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
