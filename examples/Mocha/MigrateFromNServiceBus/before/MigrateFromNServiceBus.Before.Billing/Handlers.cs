using NServiceBus;
using Shop.Contracts;

namespace Shop.Billing;

public class OrderPlacedHandler(IConfiguration configuration, ILogger<OrderPlacedHandler> logger)
    : IHandleMessages<OrderPlaced>
{
    public async Task Handle(OrderPlaced message, IMessageHandlerContext context)
    {
        var sql = context.SynchronizedStorageSession.SqlPersistenceSession();
        await using var command = sql.Connection.CreateCommand();
        command.Transaction = sql.Transaction;
        command.CommandText = "INSERT INTO invoices (order_id, status) VALUES (@id, 'Created')";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "id";
        parameter.Value = message.OrderId;
        command.Parameters.Add(parameter);
        await command.ExecuteNonQueryAsync(context.CancellationToken);
        logger.LogInformation("Invoice created for order {OrderId}", message.OrderId);
        if (message.ConfirmInvoice)
        {
            var options = new SendOptions();
            options.DelayDeliveryWith(TimeSpan.FromSeconds(configuration.GetValue<int>("Scenario:ConfirmationDelaySeconds")));
            await context.Send(new InvoiceConfirmed(message.OrderId), options);
        }
    }
}

public class CheckInvoiceStatusHandler : IHandleMessages<CheckInvoiceStatus>
{
    public async Task Handle(CheckInvoiceStatus message, IMessageHandlerContext context)
    {
        var sql = context.SynchronizedStorageSession.SqlPersistenceSession();
        await using var command = sql.Connection.CreateCommand();
        command.Transaction = sql.Transaction;
        command.CommandText = "SELECT status FROM invoices WHERE order_id = @id";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "id";
        parameter.Value = message.OrderId;
        command.Parameters.Add(parameter);
        var status = (string?)await command.ExecuteScalarAsync(context.CancellationToken) ?? "NotFound";
        await context.Reply(new InvoiceStatus(message.OrderId, status));
    }
}
