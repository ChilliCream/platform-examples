using Bookshop.Domain.Customers;

namespace Bookshop.Application.Tests.Customers.Fakes;

public sealed class InMemoryCustomerRepository : ICustomerRepository
{
    private readonly Dictionary<Guid, Customer> _customers = [];

    public Task<Customer?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_customers.GetValueOrDefault(id));

    public Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        _customers[customer.Id] = customer;
        return Task.CompletedTask;
    }
}
