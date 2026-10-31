using Mocha.Mediator;
using Microsoft.EntityFrameworkCore;
using Shop.Ordering;

// Configure the application and services.
var builder = WebApplication.CreateBuilder(args);

// Register the ordering database.
builder.AddNpgsqlDbContext<OrderingDbContext>("ordering-db");

// Register the mediator, its request pipeline, and concurrent notification publishing.
builder.Services
    .AddMediator()
    .ConfigureOptions(options => options.NotificationPublishMode = NotificationPublishMode.Concurrent)
    .AddOrdering()
    .Use(LoggingPreProcessorMiddleware.Create())
    .Use(OrderExceptionMiddleware.Create())
    .Use(LoggingMiddleware.Create())
    .Use(ValidationMiddleware.Create())
    .Use(LoggingPostProcessorMiddleware.Create())
    .Use(TransactionMiddleware.Create());

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
