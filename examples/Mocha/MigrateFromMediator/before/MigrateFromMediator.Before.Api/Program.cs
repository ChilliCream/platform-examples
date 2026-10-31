using Mediator;
using Microsoft.EntityFrameworkCore;
using Shop.Billing;
using Shop.Api.Endpoints;
using Shop.Ordering;

// Configure the API and its database.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("orders")));

// Register handlers from both assemblies and order the request pipeline.
builder.Services.AddMediator((MediatorOptions options) =>
{
    options.Assemblies = [typeof(PlaceOrderCommand), typeof(OrderBillingHandler)];
    options.ServiceLifetime = ServiceLifetime.Singleton;
    options.NotificationPublisherType = typeof(TaskWhenAllPublisher);
    options.PipelineBehaviors =
    [
        typeof(LoggingBehavior<,>),
        typeof(StartingProcessor<,>),
        typeof(ValidationBehavior<,>),
        typeof(OrderNotFoundProcessor<,>),
        typeof(TransactionBehavior<,>),
        typeof(FinishedProcessor<,>)
    ];
});

// Create the business tables before accepting requests.
var app = builder.Build();

await using (var db = await app.Services.GetRequiredService<IDbContextFactory<OrderDbContext>>().CreateDbContextAsync())
{
    await db.Database.EnsureCreatedAsync();
}

// Map the same HTTP endpoints in both apps.
app.MapOrderEndpoints();

// Start the API.
app.Run();
