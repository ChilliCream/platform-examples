using Mocha;
using Mocha.Sagas;

namespace Shop.Contracts;

public sealed class SendReminder
{
    public Guid OrderId { get; set; }
}
