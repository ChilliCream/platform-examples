using Paramore.Brighter;

namespace Shop.Contracts;

public sealed class SendReminder : Command
{
    public SendReminder() : base(Id.Random()) { }

    public Guid OrderId { get; set; }
}
