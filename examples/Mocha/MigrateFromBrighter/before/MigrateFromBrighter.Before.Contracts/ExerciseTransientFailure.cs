using Paramore.Brighter;

namespace Shop.Contracts;

/// <summary>
/// Requests a simulated transient billing failure for an order.
/// </summary>
public sealed class ExerciseTransientFailure : Command
{
    public ExerciseTransientFailure() : base(Id.Random()) { }

    /// <summary>
    /// Gets the order identifier.
    /// </summary>
    public Guid OrderId { get; set; }
}
