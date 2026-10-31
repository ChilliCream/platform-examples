using Paramore.Brighter;

namespace Shop.Contracts;

public sealed class ExpireOrder : Command
{
    public ExpireOrder() : base(Id.Random()) { }

    public Guid OrderId { get; set; }
}
