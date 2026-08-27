using Bookshop.Domain.Common;

namespace Bookshop.Domain.Orders;

/// <summary>
/// Raised when a new order is placed.
/// </summary>
public sealed record OrderPlaced(Guid OrderId, Guid CustomerId, decimal Total, string Currency)
    : IDomainEvent;
