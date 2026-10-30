using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Shop.Ordering;

// Commit command changes together after the handler succeeds.
public sealed class TransactionBehavior<TRequest, TResponse>(
    OrderingDbContext db,
    ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IOrderCommand)
        {
            return await next();
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        Logs.TransactionStarted(logger, typeof(TRequest).Name);

        var result = await next();

        await transaction.CommitAsync(cancellationToken);
        Logs.TransactionCommitted(logger, typeof(TRequest).Name);

        return result;
    }
}

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Information, "Transaction started for {Message}")]
    public static partial void TransactionStarted(ILogger logger, string message);

    [LoggerMessage(LogLevel.Information, "Transaction committed for {Message}")]
    public static partial void TransactionCommitted(ILogger logger, string message);
}
