using Bookshop.Application.Common;
using Bookshop.Domain.Common;

namespace Bookshop.Application.Tests.Common.Fakes;

/// <summary>
/// Records every domain event dispatched through it, so tests can assert which domain events a
/// handler raised without depending on Mocha's notification pipeline.
/// </summary>
public sealed class RecordingDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly List<IDomainEvent> _dispatched = [];

    public IReadOnlyList<IDomainEvent> Dispatched => _dispatched;

    public ValueTask DispatchAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken)
    {
        _dispatched.AddRange(domainEvents);
        return ValueTask.CompletedTask;
    }
}
