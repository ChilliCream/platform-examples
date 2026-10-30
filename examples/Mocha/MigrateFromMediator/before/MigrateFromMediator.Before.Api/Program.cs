using System.ComponentModel.DataAnnotations;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shop.Billing;
using Shop.Ordering;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("orders")));

builder.Services.AddMediator((MediatorOptions options) =>
{
    options.Assemblies = [typeof(PlaceOrder), typeof(OrderBillingHandler)];
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

var app = builder.Build();

await using (var db = await app.Services.GetRequiredService<IDbContextFactory<OrderDbContext>>().CreateDbContextAsync())
{
    await db.Database.EnsureCreatedAsync();
}

app.MapPost("/orders", async (PlaceOrder command, ISender sender, CancellationToken cancellationToken) =>
{
    try
    {
        var id = await sender.Send(command, cancellationToken);
        return Results.Created($"/orders/{id}", new { orderId = id });
    }
    catch (ValidationException exception)
    {
        return Results.BadRequest(new { error = exception.Message });
    }
});

app.MapGet("/orders/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
{
    var order = await sender.Send(new GetOrder(id), cancellationToken);
    return order is null ? Results.NotFound() : Results.Ok(order);
});

app.MapPost("/orders/{id:guid}/cancel", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
{
    try
    {
        await sender.Send(new CancelOrder(id), cancellationToken);
        return Results.NoContent();
    }
    catch (ValidationException exception)
    {
        return Results.BadRequest(new { error = exception.Message });
    }
    catch (OrderNotFoundException)
    {
        return Results.NotFound();
    }
});

app.Run();
