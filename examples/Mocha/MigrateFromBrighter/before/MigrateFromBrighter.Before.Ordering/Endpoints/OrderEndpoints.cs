using Microsoft.EntityFrameworkCore;
using Paramore.Brighter;
using Shop.Contracts;

namespace Shop.Ordering;

/// <summary>
/// Maps the HTTP endpoints for orders and billing scenarios.
/// </summary>
public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        app.MapPost("/orders", async (PlaceOrderRequest input, IAmACommandProcessor processor, CancellationToken ct) =>
        {
            var orderId = Guid.NewGuid();
            await processor.SendAsync(new PlaceOrderCommand(orderId, input.Product, input.ConfirmInvoice), cancellationToken: ct);
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
        app.MapGet("/orders/{orderId:guid}/invoice", (Guid orderId, IAmACommandProcessor processor) =>
        {
            var result = processor.Call<CheckInvoiceStatus, InvoiceStatus>(
                new CheckInvoiceStatus { OrderId = orderId }, timeOut: TimeSpan.FromSeconds(5));
            if (result is null)
            {
                return Results.StatusCode(504);
            }

            return Results.Ok(new
            {
                result.OrderId,
                result.Status
            });
        });

        app.MapPost("/orders/{orderId:guid}/transient", async (
            Guid orderId,
            IAmACommandProcessor processor,
            CancellationToken ct) =>
        {
            await processor.PostAsync(new ExerciseTransientFailure { OrderId = orderId }, cancellationToken: ct);
            return Results.Accepted();
        });

        app.MapPost("/orders/{orderId:guid}/invalid", async (
            Guid orderId,
            IAmACommandProcessor processor,
            CancellationToken ct) =>
        {
            await processor.PostAsync(new ExerciseValidationFailure { OrderId = orderId }, cancellationToken: ct);
            return Results.Accepted();
        });
    }
}
