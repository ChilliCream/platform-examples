using MediatR;

namespace Shop.Ordering;

/// <summary>
/// Rejects an invalid <see cref="PlaceOrderCommand"/> before a transaction is opened.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is PlaceOrderCommand command && (command.ProductId == Guid.Empty || command.Quantity <= 0))
        {
            throw new OrderValidationException("A product ID and a positive quantity are required.");
        }

        return next();
    }
}
