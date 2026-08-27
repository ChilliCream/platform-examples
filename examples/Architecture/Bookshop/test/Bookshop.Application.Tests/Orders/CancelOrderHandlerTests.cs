using Bookshop.Application.Common;
using Bookshop.Application.Orders;
using Bookshop.Application.Tests.Common.Fakes;
using Bookshop.Application.Tests.Orders.Fakes;
using Bookshop.Domain.Customers;
using Bookshop.Domain.Orders;

namespace Bookshop.Application.Tests.Orders;

public sealed class CancelOrderHandlerTests
{
    private static Order CreatePendingOrder(Guid bookId)
        => Order.Place(Guid.NewGuid(), [OrderLine.Create(bookId, 1, 10m, "USD")]);

    [Fact]
    public async Task HandleAsync_Should_CancelOrder_When_OrderIsPending()
    {
        // arrange
        var order = CreatePendingOrder(Guid.NewGuid());
        order.ClearDomainEvents();

        var orders = new InMemoryOrderRepository();
        await orders.AddAsync(order, CancellationToken.None);
        var unitOfWork = new InMemoryUnitOfWork();
        var handler = new CancelOrderHandler(orders, unitOfWork, new AllowAllAuthorizationService());

        // act
        var result = await handler.HandleAsync(new CancelOrder(order.Id), CancellationToken.None);

        // assert
        var success = result switch
        {
            Success<Guid> s => s,
            null => throw new InvalidOperationException("Result was null."),
            _ => throw new InvalidOperationException($"Expected Success but was {result}.")
        };
        Assert.Equal(order.Id, success.Value);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        var cancelled = Assert.IsType<OrderCancelled>(Assert.Single(order.DomainEvents));
        Assert.Equal(order.Id, cancelled.OrderId);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnValidationError_When_OrderIsAlreadyCancelled()
    {
        // arrange
        var order = CreatePendingOrder(Guid.NewGuid());
        order.Cancel();
        order.ClearDomainEvents();

        var orders = new InMemoryOrderRepository();
        await orders.AddAsync(order, CancellationToken.None);
        var unitOfWork = new InMemoryUnitOfWork();
        var handler = new CancelOrderHandler(orders, unitOfWork, new AllowAllAuthorizationService());

        // act
        var result = await handler.HandleAsync(new CancelOrder(order.Id), CancellationToken.None);

        // assert
        Assert.True(result is ValidationError, $"Expected ValidationError but was {result}.");
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnNotFound_When_OrderDoesNotExist()
    {
        // arrange
        var orders = new InMemoryOrderRepository();
        var unitOfWork = new InMemoryUnitOfWork();
        var handler = new CancelOrderHandler(orders, unitOfWork, new AllowAllAuthorizationService());

        // act
        var result = await handler.HandleAsync(new CancelOrder(Guid.NewGuid()), CancellationToken.None);

        // assert
        Assert.True(result is NotFound, $"Expected NotFound but was {result}.");
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }
}
