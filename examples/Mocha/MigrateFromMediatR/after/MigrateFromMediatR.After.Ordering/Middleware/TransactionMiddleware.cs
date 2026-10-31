using Mocha.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Shop.Ordering;

// Commit command changes together after the handler succeeds.
public sealed class TransactionMiddleware
{
    public async ValueTask InvokeAsync(IMediatorContext context, MediatorDelegate next)
    {
        var db = context.Services.GetRequiredService<OrderingDbContext>();
        var logger = context.Services.GetRequiredService<ILogger<TransactionMiddleware>>();
        await using var transaction = await db.Database.BeginTransactionAsync(context.CancellationToken);
        Logs.TransactionStarted(logger, context.MessageType.Name);

        await next(context);

        await transaction.CommitAsync(context.CancellationToken);
        Logs.TransactionCommitted(logger, context.MessageType.Name);
    }

    public static MediatorMiddlewareConfiguration Create()
        => new(static (factoryCtx, next) =>
        {
            // Queries and notifications do not need a command transaction.
            if (!factoryCtx.IsCommand() && !factoryCtx.IsCommandWithResponse())
            {
                return next;
            }

            var middleware = new TransactionMiddleware();
            return ctx => middleware.InvokeAsync(ctx, next);
        }, "Transaction");
}

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Information, "Transaction started for {Message}")]
    public static partial void TransactionStarted(ILogger logger, string message);

    [LoggerMessage(LogLevel.Information, "Transaction committed for {Message}")]
    public static partial void TransactionCommitted(ILogger logger, string message);
}
