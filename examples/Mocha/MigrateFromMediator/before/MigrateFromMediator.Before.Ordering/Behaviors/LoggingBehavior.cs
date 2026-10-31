using Mediator;
using Microsoft.Extensions.Logging;

namespace Shop.Ordering;

/// <summary>
/// Logs requests before the rest of the mediator pipeline. Notifications have their own handlers.
/// </summary>
public sealed class LoggingBehavior<TMessage, TResponse>(ILogger<LoggingBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse> where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        Logs.MessageHandling(logger, typeof(TMessage).Name);
        return await next(message, cancellationToken);
    }
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Handling {Message}")]
    internal static partial void MessageHandling(ILogger logger, string message);
}
