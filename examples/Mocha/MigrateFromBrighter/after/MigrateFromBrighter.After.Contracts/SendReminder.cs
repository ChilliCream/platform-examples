using Mocha;
using Mocha.Sagas;

namespace Shop.Contracts;

/// <summary>
/// Requests a reminder for an order.
/// </summary>
public sealed record SendReminder
{
    /// <summary>
    /// Gets the order identifier.
    /// </summary>
    public Guid OrderId { get; init; }
}
