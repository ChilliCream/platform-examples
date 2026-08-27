using Bookshop.Domain.Common;

namespace Bookshop.Domain.Orders;

/// <summary>
/// The order aggregate root. An order is placed with at least one line and can
/// only be cancelled while it is still pending.
/// </summary>
public sealed class Order : Entity
{
    private readonly List<OrderLine> _lines = [];

    public Guid CustomerId { get; }

    public OrderStatus Status { get; private set; }

    public IReadOnlyList<OrderLine> Lines => _lines;

    public decimal Total => _lines.Sum(line => line.LineTotal);

    public string Currency => _lines[0].Currency;

    private Order(Guid id, Guid customerId)
        : base(id)
    {
        CustomerId = customerId;
        Status = OrderStatus.Pending;
    }

    public static Order Place(Guid customerId, IReadOnlyList<OrderLine> lines)
    {
        if (lines is null || lines.Count == 0)
        {
            throw new ArgumentException("An order must contain at least one line.", nameof(lines));
        }

        if (lines.Any(line => !string.Equals(line.Currency, lines[0].Currency, StringComparison.Ordinal)))
        {
            throw new ArgumentException("All order lines must use the same currency.", nameof(lines));
        }

        var order = new Order(Guid.NewGuid(), customerId);
        order._lines.AddRange(lines);
        order.Raise(new OrderPlaced(order.Id, order.CustomerId, order.Total, order.Currency));

        return order;
    }

    public void Cancel()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Only pending orders can be cancelled; current status is {Status}.");
        }

        Status = OrderStatus.Cancelled;
        Raise(new OrderCancelled(Id));
    }

}
