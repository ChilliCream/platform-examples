using Paramore.Brighter;

namespace Shop.Contracts;

/// <summary>
/// Requests a simulated validation failure for an order.
/// </summary>
public sealed class ExerciseValidationFailure : Command
{
    public ExerciseValidationFailure() : base(Id.Random()) { }

    /// <summary>
    /// Gets the order identifier.
    /// </summary>
    public Guid OrderId { get; set; }
}
