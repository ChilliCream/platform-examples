using Microsoft.EntityFrameworkCore;
using Mocha;
using Mocha.EntityFrameworkCore;
using Mocha.Mediator;
using Mocha.Outbox;
using Mocha.Scheduling;
using Mocha.Transport.RabbitMQ;
using Npgsql;
using Shop.Contracts;
using Shop.Ordering;

var builder = WebApplication.CreateBuilder(args);
var database = builder.Configuration.GetConnectionString("ordering-db")!;
builder.Services.AddSingleton(new OrderingDataSource(NpgsqlDataSource.Create(database)));
builder.AddNpgsqlDbContext<OrderingDbContext>("ordering-db");
builder.AddRabbitMQClient("rabbitmq");
builder.Services.AddMediator().AddOrdering();
builder.Services.AddMessageBus()
    .AddOrdering()
    .AddEntityFramework<OrderingDbContext>(p =>
    {
        p.UsePostgresOutbox(o => o.RequireTransaction = false);
        p.UsePostgresScheduling();
    })
    .AddRabbitMQ();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<OrderingDbContext>().Database.EnsureCreatedAsync();
}
await OrderingDatabase.InitializeAsync(app.Services.GetRequiredService<OrderingDataSource>());

app.MapPost("/orders", async (PlaceOrderRequest input, ISender sender, CancellationToken ct) =>
{
    var orderId = Guid.NewGuid();
    await sender.SendAsync(new PlaceOrder(orderId, input.Product, input.ConfirmInvoice), ct);
    return Results.Accepted($"/orders/{orderId}", new { orderId });
});

app.MapGet("/orders/{orderId:guid}", async (Guid orderId, OrderingDataSource db, CancellationToken ct) =>
{
    await using var command = db.Value.CreateCommand("select o.product, p.status from orders o join order_processes p on p.order_id = o.id where o.id = $1");
    command.Parameters.AddWithValue(orderId);
    await using var reader = await command.ExecuteReaderAsync(ct);
    return await reader.ReadAsync(ct)
        ? Results.Ok(new { orderId, product = reader.GetString(0), status = reader.GetString(1) })
        : Results.NotFound();
});

app.MapGet("/orders/{orderId:guid}/invoice", async (Guid orderId, IMessageBus bus, CancellationToken ct) =>
{
    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
    timeout.CancelAfter(TimeSpan.FromSeconds(5));
    try
    {
        var result = await bus.RequestAsync(new CheckInvoiceStatus { OrderId = orderId }, timeout.Token);
        return Results.Ok(new { result.OrderId, result.Status });
    }
    catch (OperationCanceledException) when (!ct.IsCancellationRequested)
    {
        return Results.StatusCode(504);
    }
});

app.MapPost("/orders/{orderId:guid}/transient", async (Guid orderId, IMessageBus bus, CancellationToken ct) =>
{
    await bus.SendAsync(new ExerciseTransientFailure { OrderId = orderId }, ct);
    return Results.Accepted();
});

app.MapPost("/orders/{orderId:guid}/invalid", async (Guid orderId, IMessageBus bus, CancellationToken ct) =>
{
    await bus.SendAsync(new ExerciseValidationFailure { OrderId = orderId }, ct);
    return Results.Accepted();
});

await app.RunAsync();

public sealed record PlaceOrderRequest(string Product, bool ConfirmInvoice);
