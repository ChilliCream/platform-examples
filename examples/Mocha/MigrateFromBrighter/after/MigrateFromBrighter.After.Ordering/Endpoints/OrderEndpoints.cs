using Microsoft.EntityFrameworkCore;
using Mocha;
using Mocha.Mediator;
using Shop.Contracts;

namespace Shop.Ordering;

/// <summary>
/// Maps the HTTP endpoints for orders and billing scenarios.
/// </summary>
public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        app.MapPost("/orders", async (PlaceOrderRequest input, ISender sender, CancellationToken ct) =>
        {
            var orderId = Guid.NewGuid();
            await sender.SendAsync(new PlaceOrderCommand(orderId, input.Product, input.ConfirmInvoice), ct);
            return Results.Accepted($"/orders/{orderId}", new { orderId });
        });

        app.MapGet("/orders/{orderId:guid}", async (Guid orderId, OrderingDbContext db, CancellationToken ct) =>
        {
            var order = await (from o in db.Orders
                               join process in db.OrderProcesses on o.Id equals process.OrderId
                               where o.Id == orderId
                               select new
                               {
                                   orderId,
                                   product = o.Product,
                                   status = process.Status
                               })
                .SingleOrDefaultAsync(ct);
            if (order is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(order);
        });

        // Request the invoice from Billing and return a timeout when no reply arrives.
        app.MapGet("/orders/{orderId:guid}/invoice", async (Guid orderId, IMessageBus bus, CancellationToken ct) =>
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(5));
            try
            {
                var result = await bus.RequestAsync(new CheckInvoiceStatus { OrderId = orderId }, timeout.Token);
                return Results.Ok(new
                {
                    result.OrderId,
                    result.Status
                });
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
    }
}
