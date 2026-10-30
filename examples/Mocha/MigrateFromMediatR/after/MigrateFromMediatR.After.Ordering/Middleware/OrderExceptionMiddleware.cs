using Mocha.Mediator;

namespace Shop.Ordering;

// Recover a missing query and log a missing cancellation without handling it.
public sealed class OrderExceptionMiddleware
{
    public async ValueTask InvokeAsync(IMediatorContext context, MediatorDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (OrderNotFoundException) when (context.Message is GetOrderQuery)
        {
            var logger = context.Services.GetRequiredService<ILogger<OrderExceptionMiddleware>>();
            Logs.OrderNotFound(logger, ((GetOrderQuery)context.Message).OrderId);
            context.Result = new OrderResult(null);
        }
        catch (OrderNotFoundException) when (context.Message is CancelOrderCommand)
        {
            var logger = context.Services.GetRequiredService<ILogger<OrderExceptionMiddleware>>();
            Logs.CancelOrderNotFound(logger, ((CancelOrderCommand)context.Message).OrderId);
            throw;
        }
    }

    public static MediatorMiddlewareConfiguration Create()
        => new(static (factoryCtx, next) =>
        {
            // Only these messages have order-not-found handling.
            if (!factoryCtx.IsMessageAssignableTo<GetOrderQuery>()
                && !factoryCtx.IsMessageAssignableTo<CancelOrderCommand>())
            {
                return next;
            }

            var middleware = new OrderExceptionMiddleware();
            return ctx => middleware.InvokeAsync(ctx, next);
        }, "OrderException");
}

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Information, "Order {OrderId} was not found")]
    public static partial void OrderNotFound(ILogger logger, Guid orderId);

    [LoggerMessage(LogLevel.Information, "Could not cancel order {OrderId}")]
    public static partial void CancelOrderNotFound(ILogger logger, Guid orderId);
}
