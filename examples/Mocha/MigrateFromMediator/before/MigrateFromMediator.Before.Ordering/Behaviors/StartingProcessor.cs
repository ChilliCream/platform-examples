using Mediator;
using Microsoft.Extensions.Logging;

namespace Shop.Ordering;

// Run this pre-processing step before the request handler.
public sealed class StartingProcessor<TMessage, TResponse>(ILogger<StartingProcessor<TMessage, TResponse>> logger)
    : MessagePreProcessor<TMessage, TResponse> where TMessage : IMessage
{
    protected override ValueTask Handle(TMessage message, CancellationToken cancellationToken)
    {
        Logs.MessageStarting(logger, typeof(TMessage).Name);
        return ValueTask.CompletedTask;
    }
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Starting {Message}")]
    internal static partial void MessageStarting(ILogger logger, string message);
}
