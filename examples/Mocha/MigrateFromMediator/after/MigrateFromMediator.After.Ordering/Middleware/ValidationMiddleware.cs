using System.ComponentModel.DataAnnotations;
using Mocha.Mediator;

namespace Shop.Ordering;

/// <summary>
/// Rejects invalid order commands before a transaction begins.
/// </summary>
public sealed class ValidationMiddleware
{
    public ValueTask InvokeAsync(IMediatorContext context, MediatorDelegate next)
    {
        var error = ((IOrderCommand)context.Message).Validate();
        if (error is not null)
        {
            throw new ValidationException(error);
        }

        return next(context);
    }

    public static MediatorMiddlewareConfiguration Create()
        => new(
            static (factoryCtx, next) =>
            {
                if (!factoryCtx.IsMessageAssignableTo<IOrderCommand>())
                {
                    return next;
                }

                var middleware = new ValidationMiddleware();
                return ctx => middleware.InvokeAsync(ctx, next);
            },
            "Validation");
}
