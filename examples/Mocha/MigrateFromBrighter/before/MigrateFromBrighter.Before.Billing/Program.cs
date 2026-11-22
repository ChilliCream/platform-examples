using Microsoft.EntityFrameworkCore;
using Paramore.Brighter;
using Paramore.Brighter.Extensions;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.RMQ.Async;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Paramore.Brighter.ServiceActivator.Extensions.Hosting;
using Polly;
using Polly.Registry;
using Polly.Retry;
using Shop.Billing;
using Shop.Contracts;

// Configure the service and its database.
var builder = WebApplication.CreateBuilder(args);
builder.AddNpgsqlDbContext<BillingDbContext>("billing-db");

// Configure Brighter subscriptions, publications and retry policies.
var rabbit = new RmqMessagingGatewayConnection
{
    AmpqUri = new AmqpUriSpecification(new Uri(builder.Configuration.GetConnectionString("rabbitmq")!)),
    Exchange = new Exchange("shop")
};
var pipelines = new ResiliencePipelineRegistry<string>()
    .AddBrighterDefault();
pipelines.TryAddBuilder("TransientRetry", (pipeline, _) => pipeline.AddRetry(new RetryStrategyOptions
{
    MaxRetryAttempts = 1,
    Delay = TimeSpan.FromMilliseconds(100),
    ShouldHandle = new PredicateBuilder().Handle<TransientException>()
}));

builder.Services
    .AddConsumers(options =>
    {
        options.HandlerLifetime = ServiceLifetime.Scoped;
        options.ResiliencePipelineRegistry = pipelines;
        options.Subscriptions =
        [
            new RmqSubscription<OrderPlaced>(
                new SubscriptionName("billing.order"),
                new ChannelName("billing.order"),
                new RoutingKey("order.placed")),
            new RmqSubscription<CheckInvoiceStatus>(
                new SubscriptionName("billing.status"),
                new ChannelName("billing.status"),
                new RoutingKey("invoice.status")),
            new RmqSubscription<ExerciseTransientFailure>(
                new SubscriptionName("billing.transient"),
                new ChannelName("billing.transient"),
                new RoutingKey("billing.transient"),
                requeueCount: 3),
            new RmqSubscription<ExerciseValidationFailure>(
                new SubscriptionName("billing.invalid"),
                new ChannelName("billing.invalid"),
                new RoutingKey("billing.invalid"))
        ];
        options.DefaultChannelFactory = new ChannelFactory(new RmqMessageConsumerFactory(rabbit));
    })
    .AddProducers(options =>
    {
        options.ProducerRegistry = new RmqProducerRegistryFactory(rabbit,
        [
            new RmqPublication<InvoiceConfirmed>
            {
                Topic = new RoutingKey("invoice.confirmed"),
                MakeChannels = OnMissingChannel.Create
            },
            new RmqPublication<ExerciseTransientFailure>
            {
                Topic = new RoutingKey("billing.transient"),
                MakeChannels = OnMissingChannel.Create
            },
            new RmqPublication<InvoiceStatus>
            {
                Topic = new RoutingKey("invoice.reply"),
                MakeChannels = OnMissingChannel.Assume
            }
        ]).Create();
    })
    .AutoFromAssemblies([typeof(OrderPlaced).Assembly, typeof(OrderPlacedHandler).Assembly]);
builder.Services.AddHostedService<ServiceActivatorHostedService>();

// Create the business tables before the Brighter hosted services start.
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<BillingDbContext>().Database.EnsureCreatedAsync();
}

// Map the health endpoint and start the service.
app.MapGet("/health", () => Results.Ok());
await app.RunAsync();
