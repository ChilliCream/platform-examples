using System.ComponentModel.DataAnnotations;
using System.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mocha.Mediator;

namespace Shop.Ordering;

public sealed class LoggingMiddleware
{
    public async ValueTask InvokeAsync(IMediatorContext context, MediatorDelegate next)
    {
        var logger = context.Services.GetRequiredService<ILogger<LoggingMiddleware>>();
        logger.LogInformation("Handling {Message}", context.MessageType.Name);
        await next(context);
    }

    public static MediatorMiddlewareConfiguration Create()
        => new(
            static (factoryCtx, next) =>
            {
                if (factoryCtx.IsNotification())
                {
                    return next;
                }

                var middleware = new LoggingMiddleware();
                return ctx => middleware.InvokeAsync(ctx, next);
            },
            "Logging");
}

public sealed class StartingMiddleware
{
    public async ValueTask InvokeAsync(IMediatorContext context, MediatorDelegate next)
    {
        var logger = context.Services.GetRequiredService<ILogger<StartingMiddleware>>();
        logger.LogInformation("Starting {Message}", context.MessageType.Name);
        await next(context);
    }

    public static MediatorMiddlewareConfiguration Create()
        => new(
            static (factoryCtx, next) =>
            {
                if (factoryCtx.IsNotification())
                {
                    return next;
                }

                var middleware = new StartingMiddleware();
                return ctx => middleware.InvokeAsync(ctx, next);
            },
            "Starting");
}

public sealed class ValidationMiddleware
{
    public ValueTask InvokeAsync(IMediatorContext context, MediatorDelegate next)
    {
        var error = ((IOrderCommand)context.Message).Validate();
        if (error is not null)
        {
            throw new ValidationException(error);
        }

        return next(context);
    }

    public static MediatorMiddlewareConfiguration Create()
        => new(
            static (factoryCtx, next) =>
            {
                if (!factoryCtx.IsMessageAssignableTo<IOrderCommand>())
                {
                    return next;
                }

                var middleware = new ValidationMiddleware();
                return ctx => middleware.InvokeAsync(ctx, next);
            },
            "Validation");
}

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
            logger.LogInformation("Order not found while handling {Message}", context.MessageType.Name);
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

public sealed class TransactionMiddleware
{
    public async ValueTask InvokeAsync(IMediatorContext context, MediatorDelegate next)
    {
        var logger = context.Services.GetRequiredService<ILogger<TransactionMiddleware>>();
        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            logger.LogInformation("Transaction started for {Message}", context.MessageType.Name);
            await next(context);
            transaction.Complete();
        }

        logger.LogInformation("Transaction completed for {Message}", context.MessageType.Name);
    }

    public static MediatorMiddlewareConfiguration Create()
        => new(
            static (factoryCtx, next) =>
            {
                if (!factoryCtx.IsMessageAssignableTo<IOrderCommand>())
                {
                    return next;
                }

                var middleware = new TransactionMiddleware();
                return ctx => middleware.InvokeAsync(ctx, next);
            },
            "Transaction");
}

public sealed class FinishedMiddleware
{
    public async ValueTask InvokeAsync(IMediatorContext context, MediatorDelegate next)
    {
        await next(context);
        var logger = context.Services.GetRequiredService<ILogger<FinishedMiddleware>>();
        logger.LogInformation("Finished {Message}", context.MessageType.Name);
    }

    public static MediatorMiddlewareConfiguration Create()
        => new(
            static (factoryCtx, next) =>
            {
                if (factoryCtx.IsNotification())
                {
                    return next;
                }

                var middleware = new FinishedMiddleware();
                return ctx => middleware.InvokeAsync(ctx, next);
            },
            "Finished");
}
