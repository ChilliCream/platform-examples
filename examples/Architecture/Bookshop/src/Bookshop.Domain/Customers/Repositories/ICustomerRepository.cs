namespace Bookshop.Domain.Customers;

/// <summary>
/// Loads and stores <see cref="Customer"/> aggregate roots as whole units.
/// </summary>
public interface ICustomerRepository
{
    Task<Customer?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
}
