using Paramore.Brighter;

namespace Shop.Contracts;

/// <summary>
/// Requests a reminder for an order.
/// </summary>
public sealed class SendReminder : Command
{
    public SendReminder() : base(Id.Random()) { }

    /// <summary>
    /// Gets the order identifier.
    /// </summary>
    public Guid OrderId { get; set; }
}
