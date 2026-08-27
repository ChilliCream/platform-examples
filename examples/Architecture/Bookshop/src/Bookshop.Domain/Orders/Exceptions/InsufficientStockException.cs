using Bookshop.Domain.Common;

namespace Bookshop.Domain.Orders;

public sealed class InsufficientStockException(Guid bookId, int requested, int available)
    : DomainException(
        $"Not enough stock for book '{bookId}': requested {requested}, available {available}.")
{
    public Guid BookId { get; } = bookId;

    public int Requested { get; } = requested;

    public int Available { get; } = available;
}
