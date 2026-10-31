using Mocha;
using Mocha.Sagas;

namespace Shop.Contracts;

/// <summary>
/// Requests a simulated transient billing failure for an order.
/// </summary>
public sealed record ExerciseTransientFailure
{
    /// <summary>
    /// Gets the order identifier.
    /// </summary>
    public Guid OrderId { get; set; }
}
