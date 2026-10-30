using System.Data.Common;
using Npgsql;
using Paramore.Brighter;
using Shop.Contracts;

namespace Shop.Ordering;

public sealed class PlaceOrder(Guid orderId, string product, bool confirmInvoice) : Command(Id.Random())
{
    public Guid OrderId { get; } = orderId;
    public string Product { get; } = product;
    public bool ConfirmInvoice { get; } = confirmInvoice;
}

public sealed class PlaceOrderHandler(
    IAmATransactionConnectionProvider transactionProvider,
    IAmACommandProcessor processor,
    IConfiguration configuration,
    ILogger<PlaceOrderHandler> logger) : RequestHandlerAsync<PlaceOrder>
{
    public override async Task<PlaceOrder> HandleAsync(PlaceOrder command, CancellationToken cancellationToken = default)
    {
        var connection = await transactionProvider.GetConnectionAsync(cancellationToken);
        var transaction = await transactionProvider.GetTransactionAsync(cancellationToken);
        try
        {
            await using var sql = connection.CreateCommand();
            sql.Transaction = transaction;
            sql.CommandText = """
                insert into orders (id, product) values (@id, @product);
                insert into order_processes (order_id, status) values (@id, 'Submitted');
                """;
            AddParameter(sql, "id", command.OrderId);
            AddParameter(sql, "product", command.Product);
            await sql.ExecuteNonQueryAsync(cancellationToken);

            await processor.DepositPostAsync(new OrderPlaced
            {
                OrderId = command.OrderId,
                Product = command.Product,
                ConfirmInvoice = command.ConfirmInvoice
            }, transactionProvider, cancellationToken: cancellationToken);
            await transactionProvider.CommitAsync(cancellationToken);
        }
        catch
        {
            await transactionProvider.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            transactionProvider.Close();
        }

        await processor.PostAsync(TimeSpan.FromSeconds(configuration.GetValue<int>("Scenario:OrderTimeoutSeconds")),
            new ExpireOrder { OrderId = command.OrderId }, cancellationToken: cancellationToken);
        await processor.PostAsync(TimeSpan.FromSeconds(configuration.GetValue<int>("Scenario:ReminderDelaySeconds")),
            new SendReminder { OrderId = command.OrderId }, cancellationToken: cancellationToken);
        logger.LogInformation("Order {OrderId} submitted", command.OrderId);
        return await base.HandleAsync(command, cancellationToken);
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}

public sealed class InvoiceConfirmedHandler(OrderingDataSource db, ILogger<InvoiceConfirmedHandler> logger)
    : RequestHandlerAsync<InvoiceConfirmed>
{
    public override async Task<InvoiceConfirmed> HandleAsync(InvoiceConfirmed message, CancellationToken cancellationToken = default)
    {
        await using var command = db.Value.CreateCommand("update order_processes set status = 'Accepted' where order_id = $1 and status = 'Submitted'");
        command.Parameters.AddWithValue(message.OrderId);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 1)
        {
            logger.LogInformation("Order {OrderId} accepted", message.OrderId);
        }
        return await base.HandleAsync(message, cancellationToken);
    }
}

public sealed class ExpireOrderHandler(OrderingDataSource db, ILogger<ExpireOrderHandler> logger)
    : RequestHandlerAsync<ExpireOrder>
{
    public override async Task<ExpireOrder> HandleAsync(ExpireOrder message, CancellationToken cancellationToken = default)
    {
        await using var command = db.Value.CreateCommand("update order_processes set status = 'Expired' where order_id = $1 and status = 'Submitted'");
        command.Parameters.AddWithValue(message.OrderId);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 1)
        {
            logger.LogInformation("Order {OrderId} expired", message.OrderId);
        }
        return await base.HandleAsync(message, cancellationToken);
    }
}

public sealed class SendReminderHandler(OrderingDataSource db, ILogger<SendReminderHandler> logger)
    : RequestHandlerAsync<SendReminder>
{
    public override async Task<SendReminder> HandleAsync(SendReminder message, CancellationToken cancellationToken = default)
    {
        await using var command = db.Value.CreateCommand("insert into reminders (order_id) values ($1) on conflict do nothing");
        command.Parameters.AddWithValue(message.OrderId);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 1)
        {
            logger.LogInformation("Reminder sent for order {OrderId}", message.OrderId);
        }
        return await base.HandleAsync(message, cancellationToken);
    }
}

public sealed class InvoiceStatusHandler : RequestHandler<InvoiceStatus>
{
    public override InvoiceStatus Handle(InvoiceStatus message) => base.Handle(message);
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
