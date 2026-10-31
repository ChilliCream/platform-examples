using Paramore.Brighter;

namespace Shop.Contracts;

/// <summary>
/// Requests expiry of an unconfirmed order.
/// </summary>
public sealed class ExpireOrder : Command
{
    public ExpireOrder() : base(Id.Random()) { }

    /// <summary>
    /// Gets the order identifier.
    /// </summary>
    public Guid OrderId { get; set; }
}
