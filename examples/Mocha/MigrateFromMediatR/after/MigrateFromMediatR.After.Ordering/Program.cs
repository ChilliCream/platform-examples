using MediatR;
using MediatR.NotificationPublishers;
using Microsoft.EntityFrameworkCore;
using Shop.Ordering;

var builder = WebApplication.CreateBuilder(args);

builder.AddNpgsqlDbContext<OrderingDbContext>("ordering-db");
builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssemblyContaining<PlaceOrder>();
    configuration.NotificationPublisher = new TaskWhenAllPublisher();
    configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
    configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
    configuration.AddOpenBehavior(typeof(TransactionBehavior<,>));
    configuration.AddOpenRequestPreProcessor(typeof(LoggingPreProcessor<>));
    configuration.AddOpenRequestPostProcessor(typeof(LoggingPostProcessor<,>));
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<OrderingDbContext>().Database.EnsureCreatedAsync();
}

app.MapPost("/orders", async (PlaceOrder command, ISender sender, IPublisher publisher, CancellationToken ct) =>
{
    try
    {
        var id = await sender.Send(command, ct);
        app.Logger.LogInformation("Handling {Message}", nameof(OrderPlaced));
        await publisher.Publish(new OrderPlaced(id, command.ProductId, command.Quantity), ct);
        return Results.Created($"/orders/{id}", new { id });
    }
    catch (OrderValidationException exception)
    {
        return Results.BadRequest(new { error = exception.Message });
    }
});

app.MapGet("/orders/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
{
    var result = await sender.Send(new GetOrder(id), ct);
    return result.Order is null ? Results.NotFound() : Results.Ok(result.Order);
});

app.MapPost("/orders/{id:guid}/cancel", async (Guid id, ISender sender, CancellationToken ct) =>
{
    try
    {
        await sender.Send(new CancelOrder(id), ct);
        return Results.NoContent();
    }
    catch (OrderNotFoundException)
    {
        return Results.NotFound();
    }
});

await app.RunAsync();
