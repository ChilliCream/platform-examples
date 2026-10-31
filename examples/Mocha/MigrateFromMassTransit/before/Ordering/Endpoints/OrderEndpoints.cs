using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shop.Contracts;

namespace Shop.Ordering;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        // The bus outbox saves the event with the order in one database transaction.
        app.MapPost("/orders", async (
            PlaceOrder request,
            OrderingDbContext db,
            IPublishEndpoint publisher,
            ILogger<OrderingDbContext> logger,
            CancellationToken ct) =>
        {
            var order = new Order
            {
                Id = request.OrderId,
                Amount = request.Amount,
                ConfirmInvoice = request.ConfirmInvoice
            };
            db.Orders.Add(order);
            await publisher.Publish(new OrderPlaced(order.Id, order.Amount, order.ConfirmInvoice), ct);
            await db.SaveChangesAsync(ct);

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

            return Results.Ok(new { order.Id, order.Amount, order.ConfirmInvoice, Saga = saga?.CurrentState });
        });

        // The request client waits for Billing's invoice-status response.
        app.MapGet("/orders/{id:guid}/invoice", async (
            Guid id,
            IRequestClient<CheckInvoiceStatus> client,
            CancellationToken ct) =>
        {
            var response = await client.GetResponse<InvoiceStatus>(new CheckInvoiceStatus(id), ct);
            return Results.Ok(response.Message);
        });

        app.MapPost("/orders/{id:guid}/process/{failure}", async (
            Guid id,
            string failure,
            ISendEndpointProvider provider,
            OrderingDbContext db,
            CancellationToken ct) =>
        {
            var endpoint = await provider.GetSendEndpoint(new Uri("queue:process-invoice"));
            await endpoint.Send(new ProcessInvoice(id, failure), ct);
            await db.SaveChangesAsync(ct);
            return Results.Accepted();
        });

        // Scheduled publishes also pass through the outbox.
        app.MapPost("/orders/{id:guid}/reminder", async (
            Guid id,
            IMessageScheduler scheduler,
            IConfiguration configuration,
            OrderingDbContext db,
            ILogger<OrderingDbContext> logger,
            CancellationToken ct) =>
        {
            var delay = configuration.GetValue<int>("Scenario:ReminderDelaySeconds");
            await scheduler.SchedulePublish(DateTime.UtcNow.AddSeconds(delay), new SendReminder(id), ct);
            await db.SaveChangesAsync(ct);

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
