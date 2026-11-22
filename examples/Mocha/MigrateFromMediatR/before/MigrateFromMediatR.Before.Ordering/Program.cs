using MediatR;
using MediatR.NotificationPublishers;
using Microsoft.EntityFrameworkCore;
using Shop.Ordering;

// Configure the application and services.
var builder = WebApplication.CreateBuilder(args);

// Register the ordering database.
builder.AddNpgsqlDbContext<OrderingDbContext>("ordering-db");

// Register the mediator, its request pipeline, and concurrent notification publishing.
builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssemblyContaining<PlaceOrderCommand>();
    configuration.NotificationPublisher = new TaskWhenAllPublisher();
    configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
    configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
    configuration.AddOpenBehavior(typeof(TransactionBehavior<,>));
    configuration.AddOpenRequestPreProcessor(typeof(LoggingPreProcessor<>));
    configuration.AddOpenRequestPostProcessor(typeof(LoggingPostProcessor<,>));
});

// Create the application and initialize its database.
var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<OrderingDbContext>().Database.EnsureCreatedAsync();
}

// Map the HTTP API.
app.MapOrderEndpoints();

// Start the application.
await app.RunAsync();
