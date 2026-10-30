using System.ComponentModel.DataAnnotations;
using System.Transactions;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Shop.Ordering;

public sealed class LoggingBehavior<TMessage, TResponse>(ILogger<LoggingBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse> where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling {Message}", typeof(TMessage).Name);
        return await next(message, cancellationToken);
    }
}

public sealed class ValidationBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IOrderCommand
{
    public ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var error = message.Validate();
        if (error is not null)
        {
            throw new ValidationException(error);
        }

        return next(message, cancellationToken);
    }
}

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
            logger.LogInformation("Transaction started for {Message}", typeof(TMessage).Name);
            result = await next(message, cancellationToken);
            transaction.Complete();
        }

        logger.LogInformation("Transaction completed for {Message}", typeof(TMessage).Name);
        return result;
    }
}

public sealed class OrderNotFoundProcessor<TMessage, TResponse>(ILogger<OrderNotFoundProcessor<TMessage, TResponse>> logger)
    : MessageExceptionHandler<TMessage, TResponse, OrderNotFoundException> where TMessage : IOrderCommand
{
    protected override ValueTask<ExceptionHandlingResult<TResponse>> Handle(
        TMessage message,
        OrderNotFoundException exception,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Order not found while handling {Message}", typeof(TMessage).Name);
        return NotHandled;
    }
}

public sealed class StartingProcessor<TMessage, TResponse>(ILogger<StartingProcessor<TMessage, TResponse>> logger)
    : MessagePreProcessor<TMessage, TResponse> where TMessage : IMessage
{
    protected override ValueTask Handle(TMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting {Message}", typeof(TMessage).Name);
        return ValueTask.CompletedTask;
    }
}

public sealed class FinishedProcessor<TMessage, TResponse>(ILogger<FinishedProcessor<TMessage, TResponse>> logger)
    : MessagePostProcessor<TMessage, TResponse> where TMessage : IMessage
{
    protected override ValueTask Handle(TMessage message, TResponse response, CancellationToken cancellationToken)
    {
        logger.LogInformation("Finished {Message}", typeof(TMessage).Name);
        return ValueTask.CompletedTask;
    }
}
