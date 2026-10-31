using System.Collections.Concurrent;

namespace Shop.Ordering;

public sealed class SimulatedOrderService
{
    private readonly ConcurrentDictionary<Guid, int> _failures = new();

    // Simulate an external service that fails three times before it recovers.
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
