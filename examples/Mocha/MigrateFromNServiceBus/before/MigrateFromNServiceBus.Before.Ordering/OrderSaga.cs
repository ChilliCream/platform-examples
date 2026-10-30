using NServiceBus;
using Shop.Contracts;

namespace Shop.Ordering;

public class OrderSagaData : ContainSagaData
{
    public Guid OrderId { get; set; }
}

public class OrderSaga(IConfiguration configuration, ILogger<OrderSaga> logger)
    : Saga<OrderSagaData>, IAmStartedByMessages<OrderPlaced>,
        IHandleMessages<InvoiceConfirmed>, IHandleTimeouts<OrderExpired>
{
    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<OrderSagaData> mapper)
        => mapper.MapSaga(s => s.OrderId)
            .ToMessage<OrderPlaced>(m => m.OrderId)
            .ToMessage<InvoiceConfirmed>(m => m.OrderId);

    public async Task Handle(OrderPlaced message, IMessageHandlerContext context)
    {
        await RequestTimeout<OrderExpired>(context,
            TimeSpan.FromSeconds(configuration.GetValue<int>("Scenario:SagaTimeoutSeconds")));
        logger.LogInformation("Order {OrderId} saga submitted", message.OrderId);
    }

    public async Task Handle(InvoiceConfirmed message, IMessageHandlerContext context)
    {
        await SetStatus(context, "Accepted");
        logger.LogInformation("Order {OrderId} accepted", message.OrderId);
        MarkAsComplete();
    }

    public async Task Timeout(OrderExpired state, IMessageHandlerContext context)
    {
        await SetStatus(context, "Expired");
        logger.LogInformation("Order {OrderId} expired", Data.OrderId);
        MarkAsComplete();
    }

    private async Task SetStatus(IMessageHandlerContext context, string status)
    {
        var sql = context.SynchronizedStorageSession.SqlPersistenceSession();
        await using var command = sql.Connection.CreateCommand();
        command.Transaction = sql.Transaction;
        command.CommandText = "UPDATE orders SET status = @status WHERE id = @id";
        var statusParameter = command.CreateParameter();
        statusParameter.ParameterName = "status";
        statusParameter.Value = status;
        command.Parameters.Add(statusParameter);
        var idParameter = command.CreateParameter();
        idParameter.ParameterName = "id";
        idParameter.Value = Data.OrderId;
        command.Parameters.Add(idParameter);
        await command.ExecuteNonQueryAsync(context.CancellationToken);
    }
}

public class OrderExpired;
