using System.ComponentModel.DataAnnotations.Schema;

namespace Bookshop.Domain.Common;

/// <summary>
/// Base type for domain entities. It owns the entity identifier and the transient domain-event
/// buffer so aggregates do not each reimplement that lifecycle.
/// </summary>
[NotMapped]
public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected Entity(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
