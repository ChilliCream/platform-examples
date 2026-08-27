using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mocha.Mediator;

namespace Bookshop.Infrastructure.Common.Database;

/// <summary>
/// Wraps each Mocha command in the database execution strategy and a transaction. Query and
/// notification pipelines omit this middleware at compile time.
/// </summary>
internal sealed class TransactionMiddleware
{
    public async ValueTask InvokeAsync(IMediatorContext context, MediatorDelegate next)
    {
        var dbContext = context.Services.GetRequiredService<BookshopContext>();

        if (dbContext.Database.CurrentTransaction is not null)
        {
            await next(context);
            return;
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async cancellationToken =>
        {
            await using var transaction =
                await dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await next(context);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }, context.CancellationToken);
    }

    public static MediatorMiddlewareConfiguration Create()
        => new(
            static (factoryContext, next) =>
            {
                if (factoryContext.IsQuery() || factoryContext.IsNotification())
                {
                    return next;
                }

                var middleware = new TransactionMiddleware();
                return context => middleware.InvokeAsync(context, next);
            },
            "BookshopTransaction");
}
