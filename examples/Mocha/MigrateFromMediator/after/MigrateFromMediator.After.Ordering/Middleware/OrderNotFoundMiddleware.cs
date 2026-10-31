using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mocha.Mediator;

namespace Shop.Ordering;

// Observe a missing order and leave the exception unhandled for the API.
public sealed class OrderNotFoundMiddleware
{
    public async ValueTask InvokeAsync(IMediatorContext context, MediatorDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (OrderNotFoundException)
        {
            var logger = context.Services.GetRequiredService<ILogger<OrderNotFoundMiddleware>>();
            Logs.OrderNotFound(logger, context.MessageType.Name);
            throw;
        }
    }

    public static MediatorMiddlewareConfiguration Create()
        => new(
            static (factoryCtx, next) =>
            {
                if (!factoryCtx.IsMessageAssignableTo<IOrderCommand>())
                {
                    return next;
                }

                var middleware = new OrderNotFoundMiddleware();
                return ctx => middleware.InvokeAsync(ctx, next);
            },
            "OrderNotFound");
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Order not found while handling {Message}")]
    internal static partial void OrderNotFound(ILogger logger, string message);
}
