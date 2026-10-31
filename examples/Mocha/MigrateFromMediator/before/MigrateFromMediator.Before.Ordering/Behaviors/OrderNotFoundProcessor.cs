using Mediator;
using Microsoft.Extensions.Logging;

namespace Shop.Ordering;

// Observe a missing order and leave the exception unhandled for the API.
public sealed class OrderNotFoundProcessor<TMessage, TResponse>(
    ILogger<OrderNotFoundProcessor<TMessage, TResponse>> logger)
    : MessageExceptionHandler<TMessage, TResponse, OrderNotFoundException> where TMessage : IOrderCommand
{
    protected override ValueTask<ExceptionHandlingResult<TResponse>> Handle(
        TMessage message,
        OrderNotFoundException exception,
        CancellationToken cancellationToken)
    {
        Logs.OrderNotFound(logger, typeof(TMessage).Name);
        return NotHandled;
    }
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Order not found while handling {Message}")]
    internal static partial void OrderNotFound(ILogger logger, string message);
}
