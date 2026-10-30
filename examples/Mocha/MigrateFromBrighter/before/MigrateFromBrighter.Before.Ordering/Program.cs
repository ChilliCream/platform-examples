using Npgsql;
using Paramore.Brighter;
using Paramore.Brighter.BoxProvisioning;
using Paramore.Brighter.BoxProvisioning.PostgreSql;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.RMQ.Async;
using Paramore.Brighter.Outbox.Hosting;
using Paramore.Brighter.Outbox.PostgreSql;
using Paramore.Brighter.PostgreSql;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Paramore.Brighter.ServiceActivator.Extensions.Hosting;
using Shop.Contracts;
using Shop.Ordering;

var builder = WebApplication.CreateBuilder(args);
var database = builder.Configuration.GetConnectionString("ordering-db")!;
var rabbit = new RmqMessagingGatewayConnection
{
    AmpqUri = new AmqpUriSpecification(new Uri(builder.Configuration.GetConnectionString("rabbitmq")!)),
    Exchange = new Exchange("shop")
};
var outbox = new RelationalDatabaseConfiguration(database);
var producerRegistry = new RmqProducerRegistryFactory(rabbit,
[
    new RmqPublication<OrderPlaced> { Topic = new RoutingKey("order.placed"), MakeChannels = OnMissingChannel.Create },
    new RmqPublication<ExpireOrder> { Topic = new RoutingKey("order.expire"), MakeChannels = OnMissingChannel.Create },
    new RmqPublication<SendReminder> { Topic = new RoutingKey("order.reminder"), MakeChannels = OnMissingChannel.Create },
    new RmqPublication<ExerciseTransientFailure> { Topic = new RoutingKey("billing.transient"), MakeChannels = OnMissingChannel.Create },
    new RmqPublication<ExerciseValidationFailure> { Topic = new RoutingKey("billing.invalid"), MakeChannels = OnMissingChannel.Create },
    new RmqPublication<CheckInvoiceStatus> { Topic = new RoutingKey("invoice.status"), MakeChannels = OnMissingChannel.Create }
]).Create();

builder.Services.AddSingleton(new OrderingDataSource(NpgsqlDataSource.Create(database)));
builder.Services.AddSingleton<IAmARelationalDatabaseConfiguration>(outbox);
builder.Services.AddConsumers(options =>
{
    options.HandlerLifetime = ServiceLifetime.Scoped;
    options.Subscriptions =
    [
        new RmqSubscription<InvoiceConfirmed>(new SubscriptionName("ordering.confirmed"), new ChannelName("ordering.confirmed"), new RoutingKey("invoice.confirmed")),
        new RmqSubscription<ExpireOrder>(new SubscriptionName("ordering.expire"), new ChannelName("ordering.expire"), new RoutingKey("order.expire")),
        new RmqSubscription<SendReminder>(new SubscriptionName("ordering.reminder"), new ChannelName("ordering.reminder"), new RoutingKey("order.reminder"))
    ];
    options.DefaultChannelFactory = new ChannelFactory(new RmqMessageConsumerFactory(rabbit));
})
.AddProducers(options =>
{
    options.ProducerRegistry = producerRegistry;
    options.Outbox = new PostgreSqlOutbox(outbox);
    options.ConnectionProvider = typeof(PostgreSqlConnectionProvider);
    options.TransactionProvider = typeof(PostgreSqlTransactionProvider);
    options.UseRpc = true;
    options.ReplyQueueSubscriptions =
    [
        new RmqSubscription<InvoiceStatus>(new SubscriptionName("ordering.reply"), new ChannelName("ordering.reply"), new RoutingKey("invoice.reply"), messagePumpType: MessagePumpType.Reactor)
    ];
    options.ResponseChannelFactory = new ChannelFactory(new RmqMessageConsumerFactory(rabbit));
})
.AutoFromAssemblies([typeof(OrderPlaced).Assembly, typeof(PlaceOrder).Assembly])
.UseBoxProvisioning(options => options.AddPostgreSqlOutbox(outbox))
.UseOutboxSweeper(options =>
{
    options.TimerInterval = 1;
    options.MinimumMessageAge = TimeSpan.FromSeconds(1);
});
builder.Services.AddHostedService<ServiceActivatorHostedService>();

var app = builder.Build();
await OrderingDatabase.InitializeAsync(app.Services.GetRequiredService<OrderingDataSource>());

app.MapPost("/orders", async (PlaceOrderRequest input, IAmACommandProcessor processor, CancellationToken ct) =>
{
    var orderId = Guid.NewGuid();
    await processor.SendAsync(new PlaceOrder(orderId, input.Product, input.ConfirmInvoice), cancellationToken: ct);
    return Results.Accepted($"/orders/{orderId}", new { orderId });
});

app.MapGet("/orders/{orderId:guid}", async (Guid orderId, OrderingDataSource db, CancellationToken ct) =>
{
    await using var command = db.Value.CreateCommand("select o.product, p.status from orders o join order_processes p on p.order_id = o.id where o.id = $1");
    command.Parameters.AddWithValue(orderId);
    await using var reader = await command.ExecuteReaderAsync(ct);
    return await reader.ReadAsync(ct)
        ? Results.Ok(new { orderId, product = reader.GetString(0), status = reader.GetString(1) })
        : Results.NotFound();
});

app.MapGet("/orders/{orderId:guid}/invoice", (Guid orderId, IAmACommandProcessor processor) =>
{
    var result = processor.Call<CheckInvoiceStatus, InvoiceStatus>(new CheckInvoiceStatus { OrderId = orderId }, timeOut: TimeSpan.FromSeconds(5));
    return result is null ? Results.StatusCode(504) : Results.Ok(new { result.OrderId, result.Status });
});

app.MapPost("/orders/{orderId:guid}/transient", async (Guid orderId, IAmACommandProcessor processor, CancellationToken ct) =>
{
    await processor.PostAsync(new ExerciseTransientFailure { OrderId = orderId }, cancellationToken: ct);
    return Results.Accepted();
});

app.MapPost("/orders/{orderId:guid}/invalid", async (Guid orderId, IAmACommandProcessor processor, CancellationToken ct) =>
{
    await processor.PostAsync(new ExerciseValidationFailure { OrderId = orderId }, cancellationToken: ct);
    return Results.Accepted();
});

await app.RunAsync();

public sealed record PlaceOrderRequest(string Product, bool ConfirmInvoice);
