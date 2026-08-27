using Bookshop.Domain.Common;

namespace Bookshop.Domain.Orders;

/// <summary>
/// A single line item within an <see cref="Order"/>, capturing the unit price at
/// the time the order was placed.
/// </summary>
public sealed class OrderLine : Entity
{
    public Guid BookId { get; }

    public int Quantity { get; }

    public decimal UnitPrice { get; }

    public string Currency { get; }

    public decimal LineTotal => UnitPrice * Quantity;

    private OrderLine(
        Guid id,
        Guid bookId,
        int quantity,
        decimal unitPrice,
        string currency)
        : base(id)
    {
        BookId = bookId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Currency = currency;
    }

    public static OrderLine Create(Guid bookId, int quantity, decimal unitPrice, string currency)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Order line quantity must be positive.");
        }

        if (unitPrice < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), unitPrice, "Unit price cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency must not be empty.", nameof(currency));
        }

        return new OrderLine(
            Guid.NewGuid(),
            bookId,
            quantity,
            unitPrice,
            currency.Trim().ToUpperInvariant());
    }
}
