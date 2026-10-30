using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;

namespace Shop.Ordering;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling {Message}", typeof(TRequest).Name);
        return await next();
    }
}

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is PlaceOrder command && (command.ProductId == Guid.Empty || command.Quantity <= 0))
        {
            throw new OrderValidationException("A product ID and a positive quantity are required.");
        }

        return next();
    }
}

public sealed class TransactionBehavior<TRequest, TResponse>(OrderingDbContext db, ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not IOrderCommand)
        {
            return await next();
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        logger.LogInformation("Transaction started for {Message}", typeof(TRequest).Name);
        var result = await next();
        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Transaction committed for {Message}", typeof(TRequest).Name);
        return result;
    }
}

public sealed class LoggingPreProcessor<TRequest>(ILogger<LoggingPreProcessor<TRequest>> logger)
    : IRequestPreProcessor<TRequest> where TRequest : notnull
{
    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting {Message}", typeof(TRequest).Name);
        return Task.CompletedTask;
    }
}

public sealed class LoggingPostProcessor<TRequest, TResponse>(ILogger<LoggingPostProcessor<TRequest, TResponse>> logger)
    : IRequestPostProcessor<TRequest, TResponse> where TRequest : notnull
{
    public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
    {
        logger.LogInformation("Finished {Message}", typeof(TRequest).Name);
        return Task.CompletedTask;
    }
}

public sealed class GetOrderNotFoundHandler(ILogger<GetOrderNotFoundHandler> logger)
    : IRequestExceptionHandler<GetOrder, OrderResult, OrderNotFoundException>
{
    public Task Handle(GetOrder request, OrderNotFoundException exception, RequestExceptionHandlerState<OrderResult> state, CancellationToken cancellationToken)
    {
        logger.LogInformation("Order {OrderId} was not found", request.OrderId);
        state.SetHandled(new OrderResult(null));
        return Task.CompletedTask;
    }
}

public sealed class CancelOrderNotFoundAction(ILogger<CancelOrderNotFoundAction> logger)
    : IRequestExceptionAction<CancelOrder, OrderNotFoundException>
{
    public Task Execute(CancelOrder request, OrderNotFoundException exception, CancellationToken cancellationToken)
    {
        logger.LogInformation("Could not cancel order {OrderId}", request.OrderId);
        return Task.CompletedTask;
    }
}
