using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mocha.Mediator;

namespace Shop.Ordering;

// Log requests around the rest of the mediator pipeline; notifications have their own handlers.
public sealed class LoggingMiddleware
{
    public async ValueTask InvokeAsync(IMediatorContext context, MediatorDelegate next)
    {
        var logger = context.Services.GetRequiredService<ILogger<LoggingMiddleware>>();
        Logs.MessageHandling(logger, context.MessageType.Name);
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

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Handling {Message}")]
    internal static partial void MessageHandling(ILogger logger, string message);
}
