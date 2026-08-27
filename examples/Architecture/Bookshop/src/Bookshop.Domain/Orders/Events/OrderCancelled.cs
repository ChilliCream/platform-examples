using Bookshop.Domain.Common;

namespace Bookshop.Domain.Orders;

/// <summary>
/// Raised when a pending order is cancelled.
/// </summary>
public sealed record OrderCancelled(Guid OrderId) : IDomainEvent;
