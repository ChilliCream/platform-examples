using Shop.Contracts;

namespace Shop.Ordering;

/// <summary>
/// Logs batches of <see cref="AuditOrder"/> messages.
/// </summary>
public static class AuditOrderHandler
{
    // Wolverine supplies an array when the batch is full or its timeout expires.
    public static void Handle(AuditOrder[] messages, ILogger<AuditOrder> logger)
    {
        Logs.OrdersAudited(logger, messages.Length);
    }
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Audited {Count} orders")]
    public static partial void OrdersAudited(ILogger logger, int count);
}
