using Bookshop.Application.Books.ReadModels;
using Bookshop.Application.Customers.ReadModels;
using Bookshop.Application.Orders;
using Bookshop.Application.Orders.DataLoaders;
using Bookshop.Application.Orders.ReadModels;
using Bookshop.GraphQL.Errors;
using GreenDonut.Data;
using HotChocolate.Types.Relay;
using Mocha.Mediator;
using AppSuccess = Bookshop.Application.Common.Success<System.Guid>;
using AppNotFound = Bookshop.Application.Common.NotFound;
using AppValidationError = Bookshop.Application.Common.ValidationError;

namespace Bookshop.GraphQL.Orders;

[MutationType]
internal static partial class OrderMutations
{
    [Error(typeof(NotFound))]
    [Error(typeof(InsufficientStock))]
    [Error(typeof(ValidationFailed))]
    public static async Task<OrderReadModel> PlaceOrderAsync(
        [ID<CustomerReadModel>] Guid customerId,
        IReadOnlyList<PlaceOrderLineInput> lines,
        IMediator mediator,
        QueryContext<OrderReadModel> query,
        IOrderByIdDataLoader orderById,
        CancellationToken cancellationToken)
    {
        var command = new PlaceOrder(
            customerId,
            lines.Select(line => new OrderLineRequest(line.BookId, line.Quantity)).ToList());

        var result = await mediator.SendAsync(command, cancellationToken);

        var orderId = result switch
        {
            AppSuccess success => success.Value,
            AppNotFound notFound => throw new NotFound(notFound.Reason),
            AppValidationError validation when IsInsufficientStock(validation.Reason)
                => throw new InsufficientStock(validation.Reason),
            AppValidationError validation => throw new ValidationFailed(validation.Reason),
            null => throw new InvalidOperationException("PlaceOrder result was null."),
        };

        return await orderById.With(query).LoadRequiredAsync(orderId, cancellationToken);
    }

    [Error(typeof(NotFound))]
    [Error(typeof(ValidationFailed))]
    public static async Task<OrderReadModel> CancelOrderAsync(
        [ID<OrderReadModel>] Guid orderId,
        IMediator mediator,
        QueryContext<OrderReadModel> query,
        IOrderByIdDataLoader orderById,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(new CancelOrder(orderId), cancellationToken);

        var id = result switch
        {
            AppSuccess success => success.Value,
            AppNotFound notFound => throw new NotFound(notFound.Reason),
            AppValidationError validation => throw new ValidationFailed(validation.Reason),
            null => throw new InvalidOperationException("CancelOrder result was null."),
        };

        return await orderById.With(query).LoadRequiredAsync(id, cancellationToken);
    }

    /// <summary>
    /// PlaceOrderHandler's only two ValidationError paths are "empty order lines" and
    /// insufficient stock; <c>Result&lt;Guid&gt;</c> has no finer discriminator than a
    /// free-text reason (see Bookshop.Application/Orders/PlaceOrder.cs), so insufficient-stock
    /// is classified by matching the handler's fixed message prefix. Fragile if that message
    /// ever changes; flagged on the task.
    /// </summary>
    private static bool IsInsufficientStock(string reason)
        => reason.StartsWith("Not enough stock for book", StringComparison.Ordinal);
}

/// <summary>
/// One requested line of a <c>placeOrder</c> mutation. Mirrors
/// <see cref="OrderLineRequest"/>, the Application-layer shape, but with the book id as a
/// relay global id instead of a raw GUID.
/// </summary>
public sealed record PlaceOrderLineInput([property: ID<BookReadModel>] Guid BookId, int Quantity);
