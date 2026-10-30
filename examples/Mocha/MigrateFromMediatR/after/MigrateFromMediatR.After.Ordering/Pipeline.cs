using Microsoft.EntityFrameworkCore;
using Mocha.Mediator;

namespace Shop.Ordering;

public sealed class LoggingPreProcessorMiddleware
{
    public async ValueTask InvokeAsync(IMediatorContext context, MediatorDelegate next)
    {
        var logger = context.Services.GetRequiredService<ILogger<LoggingPreProcessorMiddleware>>();
        logger.LogInformation("Starting {Message}", context.MessageType.Name);
        await next(context);
    }

    public static MediatorMiddlewareConfiguration Create()
        => new(static (factoryCtx, next) =>
        {
            if (factoryCtx.IsNotification())
            {
                return next;
            }

            var middleware = new LoggingPreProcessorMiddleware();
            return ctx => middleware.InvokeAsync(ctx, next);
        }, "PreProcessor");
}

public sealed class OrderExceptionMiddleware
{
    public async ValueTask InvokeAsync(IMediatorContext context, MediatorDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (OrderNotFoundException) when (context.Message is GetOrder)
        {
            var logger = context.Services.GetRequiredService<ILogger<OrderExceptionMiddleware>>();
            logger.LogInformation("Order {OrderId} was not found", ((GetOrder)context.Message).OrderId);
            context.Result = new OrderResult(null);
        }
        catch (OrderNotFoundException) when (context.Message is CancelOrder)
        {
            var logger = context.Services.GetRequiredService<ILogger<OrderExceptionMiddleware>>();
            logger.LogInformation("Could not cancel order {OrderId}", ((CancelOrder)context.Message).OrderId);
            throw;
        }
    }

    public static MediatorMiddlewareConfiguration Create()
        => new(static (factoryCtx, next) =>
        {
            if (!factoryCtx.IsMessageAssignableTo<GetOrder>() && !factoryCtx.IsMessageAssignableTo<CancelOrder>())
            {
                return next;
            }

            var middleware = new OrderExceptionMiddleware();
            return ctx => middleware.InvokeAsync(ctx, next);
        }, "OrderException");
}

public sealed class LoggingMiddleware
{
    public async ValueTask InvokeAsync(IMediatorContext context, MediatorDelegate next)
    {
        var logger = context.Services.GetRequiredService<ILogger<LoggingMiddleware>>();
        logger.LogInformation("Handling {Message}", context.MessageType.Name);
        await next(context);
    }

    public static MediatorMiddlewareConfiguration Create()
        => new(static (factoryCtx, next) =>
        {
            if (factoryCtx.IsNotification())
            {
                return next;
            }

            var middleware = new LoggingMiddleware();
            return ctx => middleware.InvokeAsync(ctx, next);
        }, "Logging");
}

public sealed class ValidationMiddleware
{
    public async ValueTask InvokeAsync(IMediatorContext context, MediatorDelegate next)
    {
        var command = (PlaceOrder)context.Message;
        if (command.ProductId == Guid.Empty || command.Quantity <= 0)
        {
            throw new OrderValidationException("A product ID and a positive quantity are required.");
        }

        await next(context);
    }

    public static MediatorMiddlewareConfiguration Create()
        => new(static (factoryCtx, next) =>
        {
            if (!factoryCtx.IsMessageAssignableTo<PlaceOrder>())
            {
                return next;
            }

            var middleware = new ValidationMiddleware();
            return ctx => middleware.InvokeAsync(ctx, next);
        }, "Validation");
}

public sealed class LoggingPostProcessorMiddleware
{
    public async ValueTask InvokeAsync(IMediatorContext context, MediatorDelegate next)
    {
        await next(context);
        var logger = context.Services.GetRequiredService<ILogger<LoggingPostProcessorMiddleware>>();
        logger.LogInformation("Finished {Message}", context.MessageType.Name);
    }

    public static MediatorMiddlewareConfiguration Create()
        => new(static (factoryCtx, next) =>
        {
            if (factoryCtx.IsNotification())
            {
                return next;
            }

            var middleware = new LoggingPostProcessorMiddleware();
            return ctx => middleware.InvokeAsync(ctx, next);
        }, "PostProcessor");
}

public sealed class TransactionMiddleware
{
    public async ValueTask InvokeAsync(IMediatorContext context, MediatorDelegate next)
    {
        var db = context.Services.GetRequiredService<OrderingDbContext>();
        var logger = context.Services.GetRequiredService<ILogger<TransactionMiddleware>>();
        await using var transaction = await db.Database.BeginTransactionAsync(context.CancellationToken);
        logger.LogInformation("Transaction started for {Message}", context.MessageType.Name);
        await next(context);
        await transaction.CommitAsync(context.CancellationToken);
        logger.LogInformation("Transaction committed for {Message}", context.MessageType.Name);
    }

    public static MediatorMiddlewareConfiguration Create()
        => new(static (factoryCtx, next) =>
        {
            if (!factoryCtx.IsCommand() && !factoryCtx.IsCommandWithResponse())
            {
                return next;
            }

            var middleware = new TransactionMiddleware();
            return ctx => middleware.InvokeAsync(ctx, next);
        }, "Transaction");
}
