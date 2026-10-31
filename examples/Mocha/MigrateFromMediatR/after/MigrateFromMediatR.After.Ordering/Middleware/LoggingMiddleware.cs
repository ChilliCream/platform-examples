using Mocha.Mediator;

namespace Shop.Ordering;

/// <summary>
/// Logs each request before invoking the next pipeline component.
/// </summary>
public sealed class LoggingMiddleware
{
    public async ValueTask InvokeAsync(IMediatorContext context, MediatorDelegate next)
    {
        var logger = context.Services.GetRequiredService<ILogger<LoggingMiddleware>>();
        Logs.HandlingMessage(logger, context.MessageType.Name);
        await next(context);
    }

    public static MediatorMiddlewareConfiguration Create()
        => new(static (factoryCtx, next) =>
        {
            // This request-only pipeline intentionally excludes notifications.
            if (factoryCtx.IsNotification())
            {
                return next;
            }

            var middleware = new LoggingMiddleware();
            return ctx => middleware.InvokeAsync(ctx, next);
        }, "Logging");
}

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Information, "Handling {Message}")]
    public static partial void HandlingMessage(ILogger logger, string message);
}
