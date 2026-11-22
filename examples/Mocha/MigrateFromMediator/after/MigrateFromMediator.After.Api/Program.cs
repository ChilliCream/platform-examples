using Mocha.Mediator;
using Microsoft.EntityFrameworkCore;
using Shop.Api.Endpoints;
using Shop.Ordering;

// Configure the API and its database.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("orders")));

// Register handlers from both assemblies and order the request pipeline.
builder.Services
    .AddMediator()
    .ConfigureOptions(options => options.NotificationPublishMode = NotificationPublishMode.Concurrent)
    .AddOrdering()
    .AddBilling()
    .Use(LoggingMiddleware.Create())
    .Use(StartingMiddleware.Create())
    .Use(ValidationMiddleware.Create())
    .Use(OrderNotFoundMiddleware.Create())
    .Use(TransactionMiddleware.Create())
    .Use(FinishedMiddleware.Create());

// Export mediator resources for the visualizer.
builder.Services.AddNitro().AddMochaMediator();

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
