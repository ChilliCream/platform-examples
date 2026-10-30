using Mediator;
using Microsoft.Extensions.Logging;

namespace Shop.Ordering;

// Log requests around the rest of the mediator pipeline; notifications have their own handlers.
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
