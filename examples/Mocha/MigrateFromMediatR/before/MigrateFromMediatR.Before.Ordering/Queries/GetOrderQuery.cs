using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Shop.Ordering;

public sealed record GetOrderQuery(Guid OrderId) : IRequest<OrderResult>;

public sealed record OrderResult(OrderDto? Order);

public sealed record OrderDto(Guid Id, Guid ProductId, int Quantity, string Status);

// Read the current order without tracking it for changes.
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
