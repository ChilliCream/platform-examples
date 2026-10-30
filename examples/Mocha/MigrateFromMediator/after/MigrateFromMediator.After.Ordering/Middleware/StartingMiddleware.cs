using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mocha.Mediator;

namespace Shop.Ordering;

// Run this pre-processing step before the request handler.
public sealed class StartingMiddleware
{
    public async ValueTask InvokeAsync(IMediatorContext context, MediatorDelegate next)
    {
        var logger = context.Services.GetRequiredService<ILogger<StartingMiddleware>>();
        Logs.MessageStarting(logger, context.MessageType.Name);
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

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Starting {Message}")]
    internal static partial void MessageStarting(ILogger logger, string message);
}
