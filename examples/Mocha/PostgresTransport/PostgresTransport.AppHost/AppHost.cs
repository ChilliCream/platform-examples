using ChilliCream.Nitro.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure - single Postgres instance with a shared messaging database
var postgres = builder.AddPostgres("postgres").WithPgAdmin();
var messagingDb = postgres.AddDatabase("messaging-db");

// Services
var orderService = builder
    .AddProject<Projects.PostgresTransport_OrderService>("order-service")
    .WithReference(messagingDb)
    .WaitFor(messagingDb);

var shippingService = builder
    .AddProject<Projects.PostgresTransport_ShippingService>("shipping-service")
    .WithReference(messagingDb)
    .WaitFor(messagingDb);

var notificationService = builder
    .AddProject<Projects.PostgresTransport_NotificationService>("notification-service")
    .WithReference(messagingDb)
    .WaitFor(messagingDb);

// Export service resources for the Nitro visualizer.
builder
    .AddNitroResources(port: 5107)
    .WithReference(orderService)
    .WithReference(shippingService)
    .WithReference(notificationService);

builder.Build().Run();
