using Mocha.Mediator;

namespace Shop.Ordering;

// Run before the request pipeline begins.
public sealed class LoggingPreProcessorMiddleware
{
    public async ValueTask InvokeAsync(IMediatorContext context, MediatorDelegate next)
    {
        var logger = context.Services.GetRequiredService<ILogger<LoggingPreProcessorMiddleware>>();
        Logs.StartingMessage(logger, context.MessageType.Name);
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

            var middleware = new LoggingPreProcessorMiddleware();
            return ctx => middleware.InvokeAsync(ctx, next);
        }, "PreProcessor");
}

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Information, "Starting {Message}")]
    public static partial void StartingMessage(ILogger logger, string message);
}
