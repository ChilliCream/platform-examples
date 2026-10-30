using Microsoft.EntityFrameworkCore;
using Mocha;
using Mocha.EntityFrameworkCore;
using Mocha.Inbox;
using Mocha.Outbox;
using Mocha.Sagas;
using Mocha.Scheduling;
using Mocha.Transport.RabbitMQ;
using Shop.Contracts;
using Shop.Ordering;

var builder = WebApplication.CreateBuilder(args);
builder.AddNpgsqlDbContext<OrderingDbContext>("ordering-db", settings => settings.DisableRetry = true);
builder.AddRabbitMQClient("rabbitmq");
builder.Services
    .AddMessageBus()
    .AddResilience(policy => policy.Default().Retry([TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(200)]).ThenDeadLetter())
    .AddOrdering()
    .AddEntityFramework<OrderingDbContext>(p =>
    {
        p.UsePostgresOutbox();
        p.UseTransaction();
        p.UsePostgresInbox();
        p.AddPostgresSagas();
        p.UsePostgresScheduling();
    })
    .AddRabbitMQ();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<OrderingDbContext>().Database.EnsureCreatedAsync();
}

app.MapPost("/orders", async (PlaceOrder request, OrderingDbContext db, IMessageBus bus, CancellationToken ct) =>
{
    await using var transaction = await db.Database.BeginTransactionAsync(ct);
    var order = new Order { Id = request.OrderId, Amount = request.Amount, ConfirmInvoice = request.ConfirmInvoice };
    db.Orders.Add(order);
    await bus.PublishAsync(new OrderPlaced(order.Id, order.Amount, order.ConfirmInvoice), ct);
    await db.SaveChangesAsync(ct);
    await transaction.CommitAsync(ct);
    app.Logger.LogInformation("Order {OrderId} placed", order.Id);
    return Results.Accepted($"/orders/{order.Id}", new { order.Id });
});

app.MapGet("/orders/{id:guid}", async (Guid id, OrderingDbContext db, CancellationToken ct) =>
{
    var order = await db.Orders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
    var saga = await db.OrderStates.AsNoTracking().SingleOrDefaultAsync(x => x.CorrelationId == id, ct);
    return order is null ? Results.NotFound() : Results.Ok(new { order.Id, order.Amount, order.ConfirmInvoice, Saga = saga?.CurrentState });
});

app.MapGet("/orders/{id:guid}/invoice", async (Guid id, IMessageBus bus, CancellationToken ct) =>
{
    var response = await bus.RequestAsync(new CheckInvoiceStatus(id), ct);
    return Results.Ok(response);
});

app.MapPost("/orders/{id:guid}/process/{failure}", async (Guid id, string failure, IMessageBus bus, OrderingDbContext db, CancellationToken ct) =>
{
    await using var transaction = await db.Database.BeginTransactionAsync(ct);
    await bus.SendAsync(new ProcessInvoice(id, failure), ct);
    await db.SaveChangesAsync(ct);
    await transaction.CommitAsync(ct);
    return Results.Accepted();
});

app.MapPost("/orders/{id:guid}/reminder", async (Guid id, IMessageBus bus, IConfiguration configuration, OrderingDbContext db, CancellationToken ct) =>
{
    await using var transaction = await db.Database.BeginTransactionAsync(ct);
    var delay = configuration.GetValue<int>("Scenario:ReminderDelaySeconds");
    await bus.SchedulePublishAsync(new SendReminder(id), DateTimeOffset.UtcNow.AddSeconds(delay), ct);
    await db.SaveChangesAsync(ct);
    await transaction.CommitAsync(ct);
    app.Logger.LogInformation("Reminder for order {OrderId} scheduled", id);
    return Results.Accepted();
});

app.Run();

public record PlaceOrder(Guid OrderId, decimal Amount, bool ConfirmInvoice);
