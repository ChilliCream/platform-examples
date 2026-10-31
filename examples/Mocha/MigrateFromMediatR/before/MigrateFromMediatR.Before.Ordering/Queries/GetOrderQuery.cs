using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Shop.Ordering;

/// <summary>
/// Requests the current details of an <see cref="Order"/>.
/// </summary>
/// <param name="OrderId">The ID of the order to retrieve.</param>
public sealed record GetOrderQuery(Guid OrderId) : IRequest<OrderResult>;

/// <summary>
/// Contains the result of a <see cref="GetOrderQuery"/>.
/// </summary>
/// <param name="Order">The order details, or <see langword="null"/> when the order was not found.</param>
public sealed record OrderResult(OrderDto? Order);

/// <summary>
/// Contains the current details of an <see cref="Order"/>.
/// </summary>
/// <param name="Id">The order ID.</param>
/// <param name="ProductId">The ID of the ordered product.</param>
/// <param name="Quantity">The number of units ordered.</param>
/// <param name="Status">The current order status.</param>
public sealed record OrderDto(Guid Id, Guid ProductId, int Quantity, string Status);

/// <summary>
/// Reads the current order without tracking it for changes.
/// </summary>
public sealed class GetOrderHandler(OrderingDbContext db) : IRequestHandler<GetOrderQuery, OrderResult>
{
    public async Task<OrderResult> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .AsNoTracking()
            .SingleOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            throw new OrderNotFoundException(request.OrderId);
        }

        return new OrderResult(new OrderDto(order.Id, order.ProductId, order.Quantity, order.Status));
    }
}
