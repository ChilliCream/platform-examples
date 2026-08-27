using Bookshop.Domain.Customers;
using Bookshop.Infrastructure.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace Bookshop.Infrastructure.Customers.Persistence;

/// <summary>
/// Loads and stores <see cref="Customer"/> aggregate roots as tracked entities. Changes are
/// persisted only when <see cref="Bookshop.Application.Common.IUnitOfWork.SaveChangesAsync"/>
/// is called; this repository never calls <c>SaveChanges</c> itself.
/// </summary>
internal sealed class CustomerRepository(BookshopContext context) : ICustomerRepository
{
    public Task<Customer?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Customers.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        context.Customers.Add(customer);
        return Task.CompletedTask;
    }
}
