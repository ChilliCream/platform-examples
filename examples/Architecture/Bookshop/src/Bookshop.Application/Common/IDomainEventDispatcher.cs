using Bookshop.Domain.Common;

namespace Bookshop.Application.Common;

/// <summary>
/// Dispatches the domain events an aggregate raised while handling a command, after the
/// aggregate's changes have been persisted.
/// </summary>
public interface IDomainEventDispatcher
{
    ValueTask DispatchAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken);
}
