using System.Transactions;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Shop.Ordering;

// Wrap order commands in an ambient transaction that completes only on success.
public sealed class TransactionBehavior<TMessage, TResponse>(ILogger<TransactionBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse> where TMessage : IOrderCommand
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        TResponse result;
        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            Logs.TransactionStarted(logger, typeof(TMessage).Name);
            result = await next(message, cancellationToken);
            transaction.Complete();
        }

        Logs.TransactionCompleted(logger, typeof(TMessage).Name);
        return result;
    }
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Transaction started for {Message}")]
    internal static partial void TransactionStarted(ILogger logger, string message);

    [LoggerMessage(Level = LogLevel.Information, Message = "Transaction completed for {Message}")]
    internal static partial void TransactionCompleted(ILogger logger, string message);
}
