using Mocha.Mediator;

namespace Shop.Ordering;

/// <summary>
/// Maps the HTTP endpoints for placing, retrieving, and cancelling orders.
/// </summary>
public static class OrderEndpoints
{
    // Dispatch requests through the mediator and publish the notification after the order is saved.
    public static void MapOrderEndpoints(this WebApplication app)
    {
        app.MapPost("/orders", async (
            PlaceOrderCommand command,
            ISender sender,
            IPublisher publisher,
            CancellationToken ct) =>
        {
            try
            {
                var id = await sender.SendAsync(command, ct);

                // Log notifications at the publish call, outside the request-only pipeline.
                Logs.HandlingNotification(app.Logger, nameof(OrderPlacedNotification));

                await publisher.PublishAsync(new OrderPlacedNotification(id, command.ProductId, command.Quantity), ct);

                return Results.Created($"/orders/{id}", new { id });
            }
            catch (OrderValidationException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
        });

        app.MapGet("/orders/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.QueryAsync(new GetOrderQuery(id), ct);

            if (result.Order is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(result.Order);
        });

        app.MapPost("/orders/{id:guid}/cancel", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            try
            {
                await sender.SendAsync(new CancelOrderCommand(id), ct);

                return Results.NoContent();
            }
            catch (OrderNotFoundException)
            {
                return Results.NotFound();
            }
        });
    }
}

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Information, "Handling {Message}")]
    public static partial void HandlingNotification(ILogger logger, string message);
}
