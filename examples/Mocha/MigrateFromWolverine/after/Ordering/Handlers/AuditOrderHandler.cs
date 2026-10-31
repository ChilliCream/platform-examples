using Mocha;
using Shop.Contracts;

namespace Shop.Ordering;

public class AuditOrderHandler(ILogger<AuditOrder> logger) : IBatchEventHandler<AuditOrder>
{
    // Mocha supplies a message batch when it is full or its timeout expires.
    public ValueTask HandleAsync(IMessageBatch<AuditOrder> messages, CancellationToken cancellationToken)
    {
        Logs.OrdersAudited(logger, messages.Count);
        return ValueTask.CompletedTask;
    }
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Audited {Count} orders")]
    public static partial void OrdersAudited(ILogger logger, int count);
}
