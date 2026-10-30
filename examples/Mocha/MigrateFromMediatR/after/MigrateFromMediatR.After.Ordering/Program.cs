using Mocha.Mediator;
using Microsoft.EntityFrameworkCore;
using Shop.Ordering;

var builder = WebApplication.CreateBuilder(args);

builder.AddNpgsqlDbContext<OrderingDbContext>("ordering-db");
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

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<OrderingDbContext>().Database.EnsureCreatedAsync();
}

app.MapPost("/orders", async (PlaceOrder command, ISender sender, IPublisher publisher, CancellationToken ct) =>
{
    try
    {
        var id = await sender.SendAsync(command, ct);
        app.Logger.LogInformation("Handling {Message}", nameof(OrderPlaced));
        await publisher.PublishAsync(new OrderPlaced(id, command.ProductId, command.Quantity), ct);
        return Results.Created($"/orders/{id}", new { id });
    }
    catch (OrderValidationException exception)
    {
        return Results.BadRequest(new { error = exception.Message });
    }
});

app.MapGet("/orders/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
{
    var result = await sender.QueryAsync(new GetOrder(id), ct);
    return result.Order is null ? Results.NotFound() : Results.Ok(result.Order);
});

app.MapPost("/orders/{id:guid}/cancel", async (Guid id, ISender sender, CancellationToken ct) =>
{
    try
    {
        await sender.SendAsync(new CancelOrder(id), ct);
        return Results.NoContent();
    }
    catch (OrderNotFoundException)
    {
        return Results.NotFound();
    }
});

await app.RunAsync();
