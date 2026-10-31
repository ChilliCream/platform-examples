using Mocha;
using Mocha.Sagas;

namespace Shop.Contracts;

/// <summary>
/// Requests a simulated validation failure for an order.
/// </summary>
public sealed record ExerciseValidationFailure
{
    /// <summary>
    /// Gets the order identifier.
    /// </summary>
    public Guid OrderId { get; set; }
}
