using Mocha.Mediator;

namespace Shop.Ordering;

/// <summary>
/// Rejects an invalid <see cref="PlaceOrderCommand"/> before a transaction is opened.
/// </summary>
public sealed class ValidationMiddleware
{
    public async ValueTask InvokeAsync(IMediatorContext context, MediatorDelegate next)
    {
        var command = (PlaceOrderCommand)context.Message;

        if (command.ProductId == Guid.Empty || command.Quantity <= 0)
        {
            throw new OrderValidationException("A product ID and a positive quantity are required.");
        }

        await next(context);
    }

    public static MediatorMiddlewareConfiguration Create()
        => new(static (factoryCtx, next) =>
        {
            // Apply validation only to the place-order command.
            if (!factoryCtx.IsMessageAssignableTo<PlaceOrderCommand>())
            {
                return next;
            }

            var middleware = new ValidationMiddleware();
            return ctx => middleware.InvokeAsync(ctx, next);
        }, "Validation");
}
