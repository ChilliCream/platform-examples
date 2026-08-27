namespace Bookshop.Application.Common;

/// <summary>
/// Dispatches domain events and persists the changes staged in the current unit of work. The
/// surrounding mediator pipeline owns the database transaction.
/// </summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
