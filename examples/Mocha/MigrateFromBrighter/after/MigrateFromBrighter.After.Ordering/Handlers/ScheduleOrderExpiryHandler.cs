using Microsoft.EntityFrameworkCore;
using Mocha;
using Mocha.Mediator;
using Shop.Contracts;

namespace Shop.Ordering;

// The initial saga transition requests scheduling after the saga state has been saved.
/// <summary>
/// Schedules an <see cref="ExpireOrder"/> request for the saga deadline.
/// </summary>
public sealed class ScheduleOrderExpiryHandler(IMessageBus bus) : IEventHandler<ScheduleOrderExpiry>
{
    public async ValueTask HandleAsync(ScheduleOrderExpiry message, CancellationToken cancellationToken)
    {
        await bus.ScheduleSendAsync(new ExpireOrder { OrderId = message.OrderId }, message.ExpiresAt, cancellationToken);
    }
}
