using Bookshop.Domain.Orders;

namespace Bookshop.Domain.Tests.Orders;

public class OrderTests
{
    private static OrderLine CreateLine(int quantity = 2, decimal unitPrice = 10m) =>
        OrderLine.Create(Guid.NewGuid(), quantity, unitPrice, "USD");

    [Fact]
    public void Place_WithAtLeastOneLine_CreatesPendingOrder()
    {
        var order = Order.Place(Guid.NewGuid(), [CreateLine()]);

        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Single(order.Lines);
    }

    [Fact]
    public void Place_WithNoLines_Throws()
    {
        Assert.Throws<ArgumentException>(() => Order.Place(Guid.NewGuid(), []));
    }

    [Fact]
    public void Place_WithNullLines_Throws()
    {
        Assert.Throws<ArgumentException>(() => Order.Place(Guid.NewGuid(), null!));
    }

    [Fact]
    public void OrderLine_Create_WithNonPositiveQuantity_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OrderLine.Create(Guid.NewGuid(), 0, 10m, "USD"));
    }

    [Fact]
    public void Total_SumsLineTotalsAcrossLines()
    {
        var lineA = CreateLine(quantity: 2, unitPrice: 10m); // 20
        var lineB = CreateLine(quantity: 3, unitPrice: 5m); // 15

        var order = Order.Place(Guid.NewGuid(), [lineA, lineB]);

        Assert.Equal(35m, order.Total);
        Assert.Equal("USD", order.Currency);
    }

    [Fact]
    public void Place_RaisesOrderPlacedEvent()
    {
        var customerId = Guid.NewGuid();
        var order = Order.Place(customerId, [CreateLine()]);

        var domainEvent = Assert.Single(order.DomainEvents);
        var orderPlaced = Assert.IsType<OrderPlaced>(domainEvent);

        Assert.Equal(order.Id, orderPlaced.OrderId);
        Assert.Equal(customerId, orderPlaced.CustomerId);
        Assert.Equal(order.Total, orderPlaced.Total);
        Assert.Equal(order.Currency, orderPlaced.Currency);
    }

    [Fact]
    public void Cancel_WhilePending_SetsStatusToCancelledAndRaisesEvent()
    {
        var order = Order.Place(Guid.NewGuid(), [CreateLine()]);
        order.ClearDomainEvents();

        order.Cancel();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        var domainEvent = Assert.Single(order.DomainEvents);
        var orderCancelled = Assert.IsType<OrderCancelled>(domainEvent);
        Assert.Equal(order.Id, orderCancelled.OrderId);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_Throws()
    {
        var order = Order.Place(Guid.NewGuid(), [CreateLine()]);
        order.Cancel();

        Assert.Throws<InvalidOperationException>(() => order.Cancel());
    }

    [Fact]
    public void ClearDomainEvents_RemovesCollectedEvents()
    {
        var order = Order.Place(Guid.NewGuid(), [CreateLine()]);

        order.ClearDomainEvents();

        Assert.Empty(order.DomainEvents);
    }
}
