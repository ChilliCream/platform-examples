using Bookshop.Application.Common;
using Bookshop.Application.Common.Authorization;
using Bookshop.Domain.Books;
using Bookshop.Domain.Orders;
using Mocha.Mediator;

namespace Bookshop.Application.Orders;

/// <summary>
/// One requested line of a <see cref="PlaceOrder"/> command: a book and the quantity to buy.
/// </summary>
public sealed record OrderLineRequest(Guid BookId, int Quantity);

/// <summary>
/// Places a new order: loads every requested book, checks and removes the requested stock,
/// then creates the order via <see cref="Order.Place"/> and persists everything atomically.
/// Fails when a book does not exist or does not have enough stock.
/// </summary>
public sealed record PlaceOrder(Guid CustomerId, IReadOnlyList<OrderLineRequest> Lines)
    : ICommand<Result<Guid>>;

public sealed class PlaceOrderHandler(
    IBookRepository books,
    IOrderRepository orders,
    IUnitOfWork unitOfWork,
    OrderService orderService,
    IAuthorizationService authorizationService)
    : ICommandHandler<PlaceOrder, Result<Guid>>
{
    public async ValueTask<Result<Guid>> HandleAsync(PlaceOrder command, CancellationToken cancellationToken)
    {
        await authorizationService.EnsurePermissionAsync(Permissions.Orders.Place, cancellationToken);

        if (command.Lines.Count == 0)
        {
            return new ValidationError("An order must contain at least one line.");
        }

        var items = new List<OrderItem>(command.Lines.Count);

        foreach (var line in command.Lines)
        {
            var book = await books.GetAsync(line.BookId, cancellationToken);

            if (book is null)
            {
                return new NotFound($"Book '{line.BookId}' was not found.");
            }

            items.Add(new OrderItem(book, line.Quantity));
        }

        Order order;
        try
        {
            order = orderService.Place(command.CustomerId, items);
        }
        catch (ArgumentException exception)
        {
            return new ValidationError(exception.Message);
        }
        catch (InsufficientStockException exception)
        {
            return new ValidationError(exception.Message);
        }
        await orders.AddAsync(order, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new Success<Guid>(order.Id);
    }
}
