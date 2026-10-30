using System.ComponentModel.DataAnnotations;
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
OrderSaga.Timeout = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("Scenario:SagaTimeoutSeconds"));
builder.Services
    .AddMessageBus()
    .Host(h => h.ServiceName("ordering"))
    .AddResilience(policy =>
    {
        policy.On<ValidationException>().DeadLetter();
        policy.Default()
            .Retry(1)
            .ThenRedeliver([TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("Scenario:RedeliveryDelaySeconds"))]);
    })
    .AddOrdering()
    .AddEntityFramework<OrderingDbContext>(p =>
    {
        p.UsePostgresOutbox(o => o.RequireTransaction = false);
        p.UseTransaction();
        p.UsePostgresInbox();
        p.AddPostgresSagas();
        p.UsePostgresScheduling();
    })
    .AddRabbitMQ();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
    await db.Database.MigrateAsync();
}
app.MapPost("/orders", async (PlaceOrder request, OrderingDbContext db, IMessageBus bus, CancellationToken ct) =>
{
    var id = request.OrderId;
    await using var transaction = await db.Database.BeginTransactionAsync(ct);
    db.Orders.Add(new Order { Id = id, Status = "Submitted" });
    await bus.PublishAsync(new OrderPlaced(id, request.ConfirmInvoice), ct);
    await db.SaveChangesAsync(ct);
    await transaction.CommitAsync(ct);
    app.Logger.LogInformation("Order {OrderId} submitted", id);
    return Results.Accepted($"/orders/{id}", new { orderId = id, status = "Submitted" });
});
app.MapGet("/orders/{id:guid}", async (Guid id, OrderingDbContext db, CancellationToken ct) =>
{
    var status = await db.Orders.Where(x => x.Id == id).Select(x => x.Status).SingleOrDefaultAsync(ct);
    return status is null ? Results.NotFound() : Results.Ok(new { orderId = id, status });
});
app.MapGet("/orders/{id:guid}/invoice", async (Guid id, IMessageBus bus, CancellationToken ct) =>
    Results.Ok(await bus.RequestAsync(new CheckInvoiceStatus(id), ct)));
app.MapPost("/orders/{id:guid}/reminder", async (Guid id, IMessageBus bus, CancellationToken ct) =>
{
    await bus.ScheduleSendAsync(
        new SendReminder(id),
        DateTimeOffset.UtcNow.AddSeconds(builder.Configuration.GetValue<int>("Scenario:ReminderDelaySeconds")),
        ct);
    return Results.Accepted();
});
app.MapPost("/failures/transient/{id:guid}", async (Guid id, IMessageBus bus, CancellationToken ct) =>
{
    await bus.SendAsync(new TryTransient(id), ct);
    return Results.Accepted();
});
app.MapPost("/failures/validation/{id:guid}", async (Guid id, IMessageBus bus, CancellationToken ct) =>
{
    await bus.SendAsync(new TryInvalid(id), ct);
    return Results.Accepted();
});
app.Run();

public record PlaceOrder(Guid OrderId, bool ConfirmInvoice);
