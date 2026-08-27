using Bookshop.Application.Common;

namespace Bookshop.Application.Tests.Common.Fakes;

/// <summary>
/// A no-op unit of work: the in-memory stores already apply their changes synchronously, so
/// there is nothing to flush.
/// </summary>
public sealed class InMemoryUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCallCount++;
        return Task.CompletedTask;
    }
}
