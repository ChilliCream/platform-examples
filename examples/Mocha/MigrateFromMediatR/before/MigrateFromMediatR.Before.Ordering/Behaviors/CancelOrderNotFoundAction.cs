using MediatR;
using MediatR.Pipeline;

namespace Shop.Ordering;

// Log a missing cancellation and leave the exception unhandled.
public sealed class CancelOrderNotFoundAction(ILogger<CancelOrderNotFoundAction> logger)
    : IRequestExceptionAction<CancelOrderCommand, OrderNotFoundException>
{
    public Task Execute(
        CancelOrderCommand request,
        OrderNotFoundException exception,
        CancellationToken cancellationToken)
    {
        Logs.CancelOrderNotFound(logger, request.OrderId);
        return Task.CompletedTask;
    }
}

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Information, "Could not cancel order {OrderId}")]
    public static partial void CancelOrderNotFound(ILogger logger, Guid orderId);
}
