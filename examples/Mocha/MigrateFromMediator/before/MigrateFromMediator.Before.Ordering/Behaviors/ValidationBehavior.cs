using System.ComponentModel.DataAnnotations;
using Mediator;

namespace Shop.Ordering;

// Reject invalid order commands before a transaction begins.
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
