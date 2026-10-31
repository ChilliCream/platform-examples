using Microsoft.Extensions.Options;
using Mocha;
using Mocha.Mediator;
using Mocha.Scheduling;
using Shop.Contracts;

namespace Shop.Ordering;

/// <summary>
/// Maps the HTTP endpoints for orders and messaging scenarios.
/// </summary>
public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        app.MapPost("/orders", async (PlaceOrderCommand command, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.SendAsync(command, ct)));

        app.MapGet("/orders/{id:guid}", async (Guid id, OrderingDb db, CancellationToken ct) =>
        {
            var order = await db.Orders.FindAsync([id], ct);
            if (order is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(order);
        });

        app.MapGet("/orders/{id:guid}/invoice", async (Guid id, IMessageBus bus, CancellationToken ct) =>
            Results.Ok(await bus.RequestAsync(new CheckInvoiceStatus(id), ct)));

        app.MapPost("/orders/{id:guid}/reminder", async (
            Guid id,
            IMessageBus bus,
            IOptions<ScenarioOptions> options,
            CancellationToken ct) =>
        {
            await bus.ScheduleSendAsync(
                new PaymentReminder(id),
                DateTimeOffset.UtcNow.AddSeconds(options.Value.ReminderSeconds),
                ct);
            return Results.Accepted();
        });

        app.MapPost("/orders/{id:guid}/fail/{failure}", async (
            Guid id,
            string failure,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            await bus.SendAsync(new FailOrder(id, failure), ct);
            return Results.Accepted();
        });

        app.MapPost("/orders/{id:guid}/audit", async (Guid id, IMessageBus bus, CancellationToken ct) =>
        {
            for (var i = 0; i < 5; i++)
            {
                await bus.SendAsync(new AuditOrder(id), ct);
            }

            return Results.Accepted();
        });
    }
}
