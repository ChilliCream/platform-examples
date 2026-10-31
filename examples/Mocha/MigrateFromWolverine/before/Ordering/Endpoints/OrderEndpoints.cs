using Microsoft.Extensions.Options;
using Shop.Contracts;
using Wolverine;

namespace Shop.Ordering;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        app.MapPost("/orders", async (PlaceOrderCommand command, IMessageBus bus, CancellationToken ct) =>
            Results.Ok(await bus.InvokeAsync<Guid>(command, ct)));

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
            Results.Ok(await bus.InvokeAsync<InvoiceStatus>(new CheckInvoiceStatus(id), ct)));

        app.MapPost("/orders/{id:guid}/reminder", async (
            Guid id,
            IMessageBus bus,
            IOptions<ScenarioOptions> options) =>
        {
            await bus.ScheduleAsync(new PaymentReminder(id), TimeSpan.FromSeconds(options.Value.ReminderSeconds));
            return Results.Accepted();
        });

        app.MapPost("/orders/{id:guid}/fail/{failure}", async (Guid id, string failure, IMessageBus bus) =>
        {
            await bus.SendAsync(new FailOrder(id, failure));
            return Results.Accepted();
        });

        app.MapPost("/orders/{id:guid}/audit", async (Guid id, IMessageBus bus) =>
        {
            for (var i = 0; i < 5; i++)
            {
                await bus.SendAsync(new AuditOrder(id));
            }

            return Results.Accepted();
        });
    }
}
