using Mediator;
using Microsoft.Extensions.Logging;

namespace Shop.Ordering;

/// <summary>
/// Logs completion after a request handler succeeds.
/// </summary>
public sealed class FinishedProcessor<TMessage, TResponse>(ILogger<FinishedProcessor<TMessage, TResponse>> logger)
    : MessagePostProcessor<TMessage, TResponse> where TMessage : IMessage
{
    protected override ValueTask Handle(TMessage message, TResponse response, CancellationToken cancellationToken)
    {
        Logs.MessageFinished(logger, typeof(TMessage).Name);
        return ValueTask.CompletedTask;
    }
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Finished {Message}")]
    internal static partial void MessageFinished(ILogger logger, string message);
}
