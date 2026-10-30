using System.ComponentModel.DataAnnotations;
using Mocha.Mediator;
using Microsoft.EntityFrameworkCore;
using Shop.Ordering;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("orders")));

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

var app = builder.Build();

await using (var db = await app.Services.GetRequiredService<IDbContextFactory<OrderDbContext>>().CreateDbContextAsync())
{
    await db.Database.EnsureCreatedAsync();
}

app.MapPost("/orders", async (PlaceOrder command, ISender sender, CancellationToken cancellationToken) =>
{
    try
    {
        var id = await sender.SendAsync(command, cancellationToken);
        return Results.Created($"/orders/{id}", new { orderId = id });
    }
    catch (ValidationException exception)
    {
        return Results.BadRequest(new { error = exception.Message });
    }
});

app.MapGet("/orders/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
{
    var order = await sender.QueryAsync(new GetOrder(id), cancellationToken);
    return order is null ? Results.NotFound() : Results.Ok(order);
});

app.MapPost("/orders/{id:guid}/cancel", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
{
    try
    {
        await sender.SendAsync(new CancelOrder(id), cancellationToken);
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
