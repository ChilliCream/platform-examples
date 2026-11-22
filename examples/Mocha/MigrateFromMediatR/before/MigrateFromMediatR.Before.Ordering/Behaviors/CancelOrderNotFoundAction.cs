using MediatR;
using MediatR.Pipeline;

namespace Shop.Ordering;

/// <summary>
/// Logs when a <see cref="CancelOrderCommand"/> cannot find the order, without handling the exception.
/// </summary>
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
