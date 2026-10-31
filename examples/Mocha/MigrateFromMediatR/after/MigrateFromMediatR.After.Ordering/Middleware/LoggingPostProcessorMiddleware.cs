using Mocha.Mediator;

namespace Shop.Ordering;

/// <summary>
/// Logs when a request completes successfully.
/// </summary>
public sealed class LoggingPostProcessorMiddleware
{
    public async ValueTask InvokeAsync(IMediatorContext context, MediatorDelegate next)
    {
        await next(context);

        var logger = context.Services.GetRequiredService<ILogger<LoggingPostProcessorMiddleware>>();
        Logs.FinishedMessage(logger, context.MessageType.Name);
    }

    public static MediatorMiddlewareConfiguration Create()
        => new(static (factoryCtx, next) =>
        {
            // This request-only pipeline intentionally excludes notifications.
            if (factoryCtx.IsNotification())
            {
                return next;
            }

            var middleware = new LoggingPostProcessorMiddleware();
            return ctx => middleware.InvokeAsync(ctx, next);
        }, "PostProcessor");
}

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Information, "Finished {Message}")]
    public static partial void FinishedMessage(ILogger logger, string message);
}
