using System.Collections.Concurrent;

namespace Shop.Billing;

public sealed class InvoiceProcessor(IConfiguration configuration)
{
    private readonly ConcurrentDictionary<Guid, DateTimeOffset> _unavailableUntil = new();

    public void Process(Guid orderId)
    {
        var now = DateTimeOffset.UtcNow;
        var duration = configuration.GetValue<int>("Scenario:DependencyOutageSeconds");
        var unavailableUntil = _unavailableUntil.GetOrAdd(orderId, now.AddSeconds(duration));
        if (now < unavailableUntil)
        {
            throw new InvalidOperationException("Temporary invoice failure");
        }

        _unavailableUntil.TryRemove(orderId, out _);
    }
}
