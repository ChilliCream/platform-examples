using System.ComponentModel.DataAnnotations;
using Npgsql;
using NpgsqlTypes;
using NServiceBus;
using NServiceBus.Persistence.Sql;
using NServiceBus.TransactionalSession;
using Shop.Contracts;
using Shop.Ordering;

[assembly: SqlPersistenceSettings(PostgreSqlScripts = true)]

var builder = WebApplication.CreateBuilder(args);
var database = builder.Configuration.GetConnectionString("ordering-db")!;
var rabbit = builder.Configuration.GetConnectionString("rabbitmq")!;

var endpoint = new EndpointConfiguration("Ordering");
endpoint.UseSerialization<SystemJsonSerializer>();
endpoint.EnableInstallers();
endpoint.EnableCallbacks();
endpoint.MakeInstanceUniquelyAddressable("ordering-1");
var rabbitTransport = new RabbitMQTransport(RoutingTopology.Conventional(QueueType.Quorum), rabbit)
{
    ManagementApiConfiguration = new ManagementApiConfiguration(builder.Configuration["RabbitMqManagementUrl"]!)
};
var transport = endpoint.UseTransport(rabbitTransport);
transport.RouteToEndpoint(typeof(CheckInvoiceStatus), "Billing");
transport.RouteToEndpoint(typeof(SendReminder), "Ordering");
transport.RouteToEndpoint(typeof(TryTransient), "Ordering");
transport.RouteToEndpoint(typeof(TryInvalid), "Ordering");
var persistence = endpoint.UsePersistence<SqlPersistence>();
var dialect = persistence.SqlDialect<SqlDialect.PostgreSql>();
dialect.JsonBParameterModifier(p => ((NpgsqlParameter)p).NpgsqlDbType = NpgsqlDbType.Jsonb);
persistence.ConnectionBuilder(() => new NpgsqlConnection(database));
persistence.EnableTransactionalSession();
endpoint.EnableOutbox();
var recoverability = endpoint.Recoverability();
recoverability.AddUnrecoverableException<ValidationException>();
recoverability.Immediate(settings => settings.NumberOfRetries(1));
recoverability.Delayed(settings =>
{
    settings.NumberOfRetries(1);
    settings.TimeIncrease(TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("Scenario:RedeliveryDelaySeconds")));
});
builder.Services.AddNServiceBusEndpoint(endpoint);

await using (var connection = new NpgsqlConnection(database))
{
    await connection.OpenAsync();
    await using var command = new NpgsqlCommand("CREATE TABLE IF NOT EXISTS orders (id uuid PRIMARY KEY, status text NOT NULL)", connection);
    await command.ExecuteNonQueryAsync();
}

var app = builder.Build();
app.MapPost("/orders", async (PlaceOrder request, ITransactionalSession session, CancellationToken ct) =>
{
    var id = request.OrderId;
    await session.Open(new SqlPersistenceOpenSessionOptions(), ct);
    var sql = session.SynchronizedStorageSession.SqlPersistenceSession();
    await using var command = sql.Connection.CreateCommand();
    command.Transaction = sql.Transaction;
    command.CommandText = "INSERT INTO orders (id, status) VALUES (@id, 'Submitted')";
    var parameter = command.CreateParameter();
    parameter.ParameterName = "id";
    parameter.Value = id;
    command.Parameters.Add(parameter);
    await command.ExecuteNonQueryAsync(ct);
    await session.Publish(new OrderPlaced(id, request.ConfirmInvoice), ct);
    await session.Commit(ct);
    app.Logger.LogInformation("Order {OrderId} submitted", id);
    return Results.Accepted($"/orders/{id}", new { orderId = id, status = "Submitted" });
});
app.MapGet("/orders/{id:guid}", async (Guid id, CancellationToken ct) =>
{
    await using var connection = new NpgsqlConnection(database);
    await connection.OpenAsync(ct);
    await using var command = new NpgsqlCommand("SELECT status FROM orders WHERE id = @id", connection);
    command.Parameters.AddWithValue("id", id);
    var status = (string?)await command.ExecuteScalarAsync(ct);
    return status is null ? Results.NotFound() : Results.Ok(new { orderId = id, status });
});
app.MapGet("/orders/{id:guid}/invoice", async (Guid id, IMessageSession session, CancellationToken ct) =>
    Results.Ok(await session.Request<InvoiceStatus>(new CheckInvoiceStatus(id), ct)));
app.MapPost("/orders/{id:guid}/reminder", async (Guid id, IMessageSession session, CancellationToken ct) =>
{
    var options = new SendOptions();
    options.DelayDeliveryWith(TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("Scenario:ReminderDelaySeconds")));
    await session.Send(new SendReminder(id), options, ct);
    return Results.Accepted();
});
app.MapPost("/failures/transient/{id:guid}", async (Guid id, IMessageSession session, CancellationToken ct) =>
{
    await session.Send(new TryTransient(id), ct);
    return Results.Accepted();
});
app.MapPost("/failures/validation/{id:guid}", async (Guid id, IMessageSession session, CancellationToken ct) =>
{
    await session.Send(new TryInvalid(id), ct);
    return Results.Accepted();
});
app.Run();

public record PlaceOrder(Guid OrderId, bool ConfirmInvoice);
