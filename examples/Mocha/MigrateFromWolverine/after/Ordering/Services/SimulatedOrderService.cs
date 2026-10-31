using System.Collections.Concurrent;

namespace Shop.Ordering;

/// <summary>
/// Simulates order processing with transient failures before recovery.
/// </summary>
public sealed class SimulatedOrderService
{
    private readonly ConcurrentDictionary<Guid, int> _failures = new();

    // Simulate an external service that fails three times before it recovers.
    // The per-order outage is intended only for this sequential scenario.
    public void Process(Guid orderId)
    {
        var failures = _failures.AddOrUpdate(orderId, 1, (_, count) => count + 1);
        if (failures <= 3)
        {
            throw new InvalidOperationException("Temporary order failure");
        }

        _failures.TryRemove(orderId, out _);
    }
}
