using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Shop.Ordering;

public interface IOrderCommand;

public sealed record PlaceOrder(Guid ProductId, int Quantity) : IRequest<Guid>, IOrderCommand;

public sealed record CancelOrder(Guid OrderId) : IRequest, IOrderCommand;

public sealed record GetOrder(Guid OrderId) : IRequest<OrderResult>;

public sealed record OrderResult(OrderDto? Order);

public sealed record OrderPlaced(Guid OrderId, Guid ProductId, int Quantity) : INotification;

public sealed record OrderDto(Guid Id, Guid ProductId, int Quantity, string Status);

public sealed class Order
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; } = "Submitted";
}

public sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
}

public sealed class OrderNotFoundException(Guid orderId) : Exception($"Order {orderId} was not found.");

public sealed class OrderValidationException(string message) : Exception(message);

public sealed class PlaceOrderHandler(OrderingDbContext db) : IRequestHandler<PlaceOrder, Guid>
{
    public async Task<Guid> Handle(PlaceOrder request, CancellationToken cancellationToken)
    {
        var order = new Order { Id = Guid.NewGuid(), ProductId = request.ProductId, Quantity = request.Quantity };
        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);
        return order.Id;
    }
}

public sealed class CancelOrderHandler(OrderingDbContext db) : IRequestHandler<CancelOrder>
{
    public async Task Handle(CancelOrder request, CancellationToken cancellationToken)
    {
        var order = await db.Orders.FindAsync([request.OrderId], cancellationToken)
            ?? throw new OrderNotFoundException(request.OrderId);
        order.Status = "Cancelled";
        await db.SaveChangesAsync(cancellationToken);
    }
}

public sealed class GetOrderHandler(OrderingDbContext db) : IRequestHandler<GetOrder, OrderResult>
{
    public async Task<OrderResult> Handle(GetOrder request, CancellationToken cancellationToken)
    {
        var order = await db.Orders.AsNoTracking().SingleOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new OrderNotFoundException(request.OrderId);
        return new OrderResult(new OrderDto(order.Id, order.ProductId, order.Quantity, order.Status));
    }
}

public sealed class OrderReceiptHandler(ILogger<OrderReceiptHandler> logger) : INotificationHandler<OrderPlaced>
{
    public Task Handle(OrderPlaced notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Receipt prepared for order {OrderId}", notification.OrderId);
        return Task.CompletedTask;
    }
}

public sealed class OrderInventoryHandler(ILogger<OrderInventoryHandler> logger) : INotificationHandler<OrderPlaced>
{
    public Task Handle(OrderPlaced notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Inventory notified for order {OrderId}", notification.OrderId);
        return Task.CompletedTask;
    }
}
