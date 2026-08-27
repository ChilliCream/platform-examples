using Bookshop.Application.Common;
using Bookshop.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Bookshop.Infrastructure.Common.Database;

internal sealed class UnitOfWork(
    BookshopContext context,
    IDomainEventDispatcher domainEventDispatcher) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        var entities = context.ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count > 0)
            .ToArray();

        var domainEvents = entities
            .SelectMany(entity => entity.DomainEvents)
            .ToArray();

        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }

        if (domainEvents.Length > 0)
        {
            await domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
