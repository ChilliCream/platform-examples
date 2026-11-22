using System.ComponentModel.DataAnnotations;
using Mediator;
using Shop.Ordering;

namespace Shop.Api.Endpoints;

/// <summary>
/// Maps the HTTP endpoints for placing, reading, and cancelling orders.
/// </summary>
public static class OrderEndpoints
{
    // The HTTP contract stays the same while the mediator dispatch methods change.
    public static void MapOrderEndpoints(this WebApplication app)
    {
        app.MapPost("/orders", async (PlaceOrderCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            try
            {
                var id = await sender.Send(command, cancellationToken);
                return Results.Created($"/orders/{id}", new { orderId = id });
            }
            catch (ValidationException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
        });

        app.MapGet("/orders/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var order = await sender.Send(new GetOrderQuery(id), cancellationToken);
            if (order is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(order);
        });

        app.MapPost("/orders/{id:guid}/cancel", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            try
            {
                await sender.Send(new CancelOrderCommand(id), cancellationToken);
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
    }
}
