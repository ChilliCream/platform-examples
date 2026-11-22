using Microsoft.EntityFrameworkCore;
using Paramore.Brighter;
using Paramore.Brighter.BoxProvisioning;
using Paramore.Brighter.BoxProvisioning.PostgreSql;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.RMQ.Async;
using Paramore.Brighter.Outbox.Hosting;
using Paramore.Brighter.Outbox.PostgreSql;
using Paramore.Brighter.PostgreSql;
using Paramore.Brighter.PostgreSql.EntityFrameworkCore;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Paramore.Brighter.ServiceActivator.Extensions.Hosting;
using Shop.Contracts;
using Shop.Ordering;

// Configure the service and its database.
var builder = WebApplication.CreateBuilder(args);
var database = builder.Configuration.GetConnectionString("ordering-db")!;
builder.AddNpgsqlDbContext<OrderingDbContext>("ordering-db");

// Brighter routes each message using a publication and subscription.
var rabbit = new RmqMessagingGatewayConnection
{
    AmpqUri = new AmqpUriSpecification(new Uri(builder.Configuration.GetConnectionString("rabbitmq")!)),
    Exchange = new Exchange("shop")
};
var outbox = new RelationalDatabaseConfiguration(database);
var producerRegistry = new RmqProducerRegistryFactory(rabbit,
[
    new RmqPublication<OrderPlaced>
    {
        Topic = new RoutingKey("order.placed"),
        MakeChannels = OnMissingChannel.Create
    },
    new RmqPublication<ExpireOrder>
    {
        Topic = new RoutingKey("order.expire"),
        MakeChannels = OnMissingChannel.Create
    },
    new RmqPublication<SendReminder>
    {
        Topic = new RoutingKey("order.reminder"),
        MakeChannels = OnMissingChannel.Create
    },
    new RmqPublication<ExerciseTransientFailure>
    {
        Topic = new RoutingKey("billing.transient"),
        MakeChannels = OnMissingChannel.Create
    },
    new RmqPublication<ExerciseValidationFailure>
    {
        Topic = new RoutingKey("billing.invalid"),
        MakeChannels = OnMissingChannel.Create
    },
    new RmqPublication<CheckInvoiceStatus>
    {
        Topic = new RoutingKey("invoice.status"),
        MakeChannels = OnMissingChannel.Create
    }
]).Create();

// The sweeper dispatches messages deposited in the transactional outbox.
builder.Services.AddSingleton<IAmARelationalDatabaseConfiguration>(outbox);
builder.Services
    .AddConsumers(options =>
    {
        options.HandlerLifetime = ServiceLifetime.Scoped;
        options.Subscriptions =
        [
            new RmqSubscription<InvoiceConfirmed>(
                new SubscriptionName("ordering.confirmed"),
                new ChannelName("ordering.confirmed"),
                new RoutingKey("invoice.confirmed")),
            new RmqSubscription<ExpireOrder>(
                new SubscriptionName("ordering.expire"),
                new ChannelName("ordering.expire"),
                new RoutingKey("order.expire")),
            new RmqSubscription<SendReminder>(
                new SubscriptionName("ordering.reminder"),
                new ChannelName("ordering.reminder"),
                new RoutingKey("order.reminder"))
        ];
        options.DefaultChannelFactory = new ChannelFactory(new RmqMessageConsumerFactory(rabbit));
    })
    .AddProducers(options =>
    {
        options.ProducerRegistry = producerRegistry;
        options.Outbox = new PostgreSqlOutbox(outbox);
        options.ConnectionProvider = typeof(PostgreSqlConnectionProvider);
        options.TransactionProvider = typeof(PostgreSqlEntityFrameworkTransactionProvider<OrderingDbContext>);
        options.UseRpc = true;
        options.ReplyQueueSubscriptions =
        [
            new RmqSubscription<InvoiceStatus>(
                new SubscriptionName("ordering.reply"),
                new ChannelName("ordering.reply"),
                new RoutingKey("invoice.reply"),
                messagePumpType: MessagePumpType.Reactor)
        ];
        options.ResponseChannelFactory = new ChannelFactory(new RmqMessageConsumerFactory(rabbit));
    })
    .AutoFromAssemblies([typeof(OrderPlaced).Assembly, typeof(PlaceOrderCommand).Assembly])
    .UseBoxProvisioning(options => options.AddPostgreSqlOutbox(outbox))
    .UseOutboxSweeper(options =>
    {
        options.TimerInterval = 1;
        options.MinimumMessageAge = TimeSpan.FromSeconds(1);
    });
builder.Services.AddHostedService<ServiceActivatorHostedService>();

// Create the business tables before the Brighter hosted services start.
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<OrderingDbContext>().Database.EnsureCreatedAsync();
}

// Map the HTTP API and start the service.
app.MapOrderEndpoints();
await app.RunAsync();
