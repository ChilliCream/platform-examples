using Mocha;
using Microsoft.EntityFrameworkCore;
using Shop.Contracts;

namespace Shop.Ordering;

/// <summary>
/// Maps HTTP endpoints for orders, invoice status, processing, and reminders.
/// </summary>
public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        // The bus outbox saves the saga start with the order in one database transaction.
        app.MapPost("/orders", async (
            PlaceOrder request,
            OrderingDbContext db,
            IMessageBus bus,
            ILogger<OrderingDbContext> logger,
            CancellationToken ct) =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            var order = new Order
            {
                Id = request.OrderId,
                Amount = request.Amount,
                ConfirmInvoice = request.ConfirmInvoice
            };
            db.Orders.Add(order);
            await bus.PublishAsync(new StartOrder(order.Id, order.Amount, order.ConfirmInvoice), ct);
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            Logs.OrderPlaced(logger, order.Id);
            return Results.Accepted($"/orders/{order.Id}", new { order.Id });
        });

        app.MapGet("/orders/{id:guid}", async (Guid id, OrderingDbContext db, CancellationToken ct) =>
        {
            var order = await db.Orders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
            var saga = await db.OrderStates.AsNoTracking().SingleOrDefaultAsync(x => x.CorrelationId == id, ct);
            if (order is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(new
            {
                order.Id,
                order.Amount,
                order.ConfirmInvoice,
                Saga = saga?.CurrentState
            });
        });

        // Request Billing's invoice status and wait for its response.
        app.MapGet("/orders/{id:guid}/invoice", async (
            Guid id,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var response = await bus.RequestAsync(new CheckInvoiceStatus(id), ct);
            return Results.Ok(response);
        });

        app.MapPost("/orders/{id:guid}/process/{failure}", async (
            Guid id,
            string failure,
            IMessageBus bus,
            OrderingDbContext db,
            CancellationToken ct) =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            await bus.SendAsync(new ProcessInvoice(id, failure), ct);
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return Results.Accepted();
        });

        // Scheduled publishes also pass through the outbox.
        app.MapPost("/orders/{id:guid}/reminder", async (
            Guid id,
            IMessageBus bus,
            IConfiguration configuration,
            OrderingDbContext db,
            ILogger<OrderingDbContext> logger,
            CancellationToken ct) =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            var delay = configuration.GetValue<int>("Scenario:ReminderDelaySeconds");
            await bus.SchedulePublishAsync(new SendReminder(id), DateTimeOffset.UtcNow.AddSeconds(delay), ct);
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            Logs.ReminderScheduled(logger, id);
            return Results.Accepted();
        });
    }
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} placed")]
    public static partial void OrderPlaced(ILogger logger, Guid orderId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Reminder for order {OrderId} scheduled")]
    public static partial void ReminderScheduled(ILogger logger, Guid orderId);
}
