using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mocha.Mediator;

namespace Shop.Ordering;

/// <summary>
/// Logs completion after a request handler succeeds.
/// </summary>
public sealed class FinishedMiddleware
{
    public async ValueTask InvokeAsync(IMediatorContext context, MediatorDelegate next)
    {
        await next(context);
        var logger = context.Services.GetRequiredService<ILogger<FinishedMiddleware>>();
        Logs.MessageFinished(logger, context.MessageType.Name);
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

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Finished {Message}")]
    internal static partial void MessageFinished(ILogger logger, string message);
}
