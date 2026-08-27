using Bookshop.Application.Common;
using Bookshop.Application.Orders;
using Bookshop.Application.Tests.Books.Fakes;
using Bookshop.Application.Tests.Common.Fakes;
using Bookshop.Application.Tests.Orders.Fakes;
using Bookshop.Domain.Authors;
using Bookshop.Domain.Books;
using Bookshop.Domain.Customers;
using Bookshop.Domain.Orders;

namespace Bookshop.Application.Tests.Orders;

public sealed class PlaceOrderHandlerTests
{
    private static Book CreateBook(int stock)
        => Book.Create("Clean Code", "9780132350884", 39.99m, "USD", Guid.NewGuid(), stock);

    [Fact]
    public async Task HandleAsync_Should_PlaceOrder_When_StockIsSufficient()
    {
        // arrange
        var books = new InMemoryBookRepository();
        var book = CreateBook(stock: 5);
        await books.AddAsync(book, CancellationToken.None);

        var orders = new InMemoryOrderRepository();
        var unitOfWork = new InMemoryUnitOfWork();
        var handler = new PlaceOrderHandler(
            books, orders, unitOfWork, new OrderService(), new AllowAllAuthorizationService());
        var customerId = Guid.NewGuid();

        // act
        var result = await handler.HandleAsync(
            new PlaceOrder(customerId, [new OrderLineRequest(book.Id, 2)]),
            CancellationToken.None);

        // assert
        var success = result switch
        {
            Success<Guid> s => s,
            null => throw new InvalidOperationException("Result was null."),
            _ => throw new InvalidOperationException($"Expected Success but was {result}.")
        };
        var storedOrder = await orders.GetAsync(success.Value, CancellationToken.None);
        Assert.NotNull(storedOrder);
        Assert.Equal(OrderStatus.Pending, storedOrder.Status);
        Assert.Single(storedOrder.Lines);
        Assert.Equal(2, storedOrder.Lines[0].Quantity);
        Assert.Equal(3, book.StockQuantity);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        var placed = Assert.IsType<OrderPlaced>(Assert.Single(storedOrder.DomainEvents));
        Assert.Equal(success.Value, placed.OrderId);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnValidationError_When_StockIsInsufficient()
    {
        // arrange
        var books = new InMemoryBookRepository();
        var book = CreateBook(stock: 1);
        await books.AddAsync(book, CancellationToken.None);

        var orders = new InMemoryOrderRepository();
        var unitOfWork = new InMemoryUnitOfWork();
        var handler = new PlaceOrderHandler(
            books, orders, unitOfWork, new OrderService(), new AllowAllAuthorizationService());

        // act
        var result = await handler.HandleAsync(
            new PlaceOrder(Guid.NewGuid(), [new OrderLineRequest(book.Id, 2)]),
            CancellationToken.None);

        // assert
        Assert.True(result is ValidationError, $"Expected ValidationError but was {result}.");
        Assert.Equal(1, book.StockQuantity);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnValidationError_When_CombinedQuantityAcrossLinesExceedsStock()
    {
        // arrange
        var books = new InMemoryBookRepository();
        var book = CreateBook(stock: 5);
        await books.AddAsync(book, CancellationToken.None);

        var orders = new InMemoryOrderRepository();
        var unitOfWork = new InMemoryUnitOfWork();
        var handler = new PlaceOrderHandler(
            books, orders, unitOfWork, new OrderService(), new AllowAllAuthorizationService());

        // act
        var result = await handler.HandleAsync(
            new PlaceOrder(
                Guid.NewGuid(),
                [new OrderLineRequest(book.Id, 3), new OrderLineRequest(book.Id, 3)]),
            CancellationToken.None);

        // assert
        Assert.True(result is ValidationError, $"Expected ValidationError but was {result}.");
        Assert.Equal(5, book.StockQuantity);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnNotFound_When_BookDoesNotExist()
    {
        // arrange
        var books = new InMemoryBookRepository();
        var orders = new InMemoryOrderRepository();
        var unitOfWork = new InMemoryUnitOfWork();
        var handler = new PlaceOrderHandler(
            books, orders, unitOfWork, new OrderService(), new AllowAllAuthorizationService());

        // act
        var result = await handler.HandleAsync(
            new PlaceOrder(Guid.NewGuid(), [new OrderLineRequest(Guid.NewGuid(), 1)]),
            CancellationToken.None);

        // assert
        Assert.True(result is NotFound, $"Expected NotFound but was {result}.");
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }
}
