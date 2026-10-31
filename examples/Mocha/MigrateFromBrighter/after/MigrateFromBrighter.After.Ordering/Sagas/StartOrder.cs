using Mocha.Sagas;
using Shop.Contracts;

namespace Shop.Ordering;

/// <summary>
/// Starts the order saga with the order details and expiry.
/// </summary>
public sealed record StartOrder
{
    /// <summary>
    /// Gets the order identifier.
    /// </summary>
    public Guid OrderId { get; init; }

    /// <summary>
    /// Gets the product to order.
    /// </summary>
    public string Product { get; init; } = string.Empty;

    /// <summary>
    /// Gets whether Billing should confirm the invoice.
    /// </summary>
    public bool ConfirmInvoice { get; init; }

    /// <summary>
    /// Gets the time at which the order expires.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; init; }
}
