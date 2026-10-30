using MediatR;
using MediatR.Pipeline;

namespace Shop.Ordering;

// Recover a missing query with an empty result.
public sealed class GetOrderNotFoundHandler(ILogger<GetOrderNotFoundHandler> logger)
    : IRequestExceptionHandler<GetOrderQuery, OrderResult, OrderNotFoundException>
{
    public Task Handle(
        GetOrderQuery request,
        OrderNotFoundException exception,
        RequestExceptionHandlerState<OrderResult> state,
        CancellationToken cancellationToken)
    {
        Logs.OrderNotFound(logger, request.OrderId);
        state.SetHandled(new OrderResult(null));
        return Task.CompletedTask;
    }
}

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Information, "Order {OrderId} was not found")]
    public static partial void OrderNotFound(ILogger logger, Guid orderId);
}
