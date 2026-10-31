using System.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mocha.Mediator;

namespace Shop.Ordering;

// Wrap order commands in an ambient transaction that completes only on success.
public sealed class TransactionMiddleware
{
    public async ValueTask InvokeAsync(IMediatorContext context, MediatorDelegate next)
    {
        var logger = context.Services.GetRequiredService<ILogger<TransactionMiddleware>>();
        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            Logs.TransactionStarted(logger, context.MessageType.Name);
            await next(context);
            transaction.Complete();
        }

        Logs.TransactionCompleted(logger, context.MessageType.Name);
    }

    public static MediatorMiddlewareConfiguration Create()
        => new(
            static (factoryCtx, next) =>
            {
                if (!factoryCtx.IsMessageAssignableTo<IOrderCommand>())
                {
                    return next;
                }

                var middleware = new TransactionMiddleware();
                return ctx => middleware.InvokeAsync(ctx, next);
            },
            "Transaction");
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Transaction started for {Message}")]
    internal static partial void TransactionStarted(ILogger logger, string message);

    [LoggerMessage(Level = LogLevel.Information, Message = "Transaction completed for {Message}")]
    internal static partial void TransactionCompleted(ILogger logger, string message);
}
