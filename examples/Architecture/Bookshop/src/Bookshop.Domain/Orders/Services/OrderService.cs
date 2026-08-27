using Bookshop.Domain.Books;

namespace Bookshop.Domain.Orders;

/// <summary>
/// Domain service for placing an order, a business operation that spans the order aggregate and
/// one or more book aggregates.
/// </summary>
public sealed class OrderService
{
    public Order Place(Guid customerId, IReadOnlyList<OrderItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count == 0)
        {
            throw new ArgumentException("An order must contain at least one line.", nameof(items));
        }

        var requestedPerBook = new Dictionary<Guid, int>();

        foreach (var item in items)
        {
            if (item.Quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(items),
                    item.Quantity,
                    "Order line quantity must be positive.");
            }

            var totalRequested = requestedPerBook.GetValueOrDefault(item.Book.Id) + item.Quantity;

            if (totalRequested > item.Book.StockQuantity)
            {
                throw new InsufficientStockException(
                    item.Book.Id,
                    totalRequested,
                    item.Book.StockQuantity);
            }

            requestedPerBook[item.Book.Id] = totalRequested;
        }

        var lines = new List<OrderLine>(items.Count);

        foreach (var item in items)
        {
            item.Book.RemoveStock(item.Quantity);
            lines.Add(OrderLine.Create(
                item.Book.Id,
                item.Quantity,
                item.Book.Price,
                item.Book.Currency));
        }

        return Order.Place(customerId, lines);
    }
}
