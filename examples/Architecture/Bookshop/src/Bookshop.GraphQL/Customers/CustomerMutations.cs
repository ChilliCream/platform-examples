using Bookshop.Application.Customers;
using Bookshop.Application.Customers.DataLoaders;
using Bookshop.Application.Customers.ReadModels;
using GreenDonut.Data;
using Mocha.Mediator;

namespace Bookshop.GraphQL.Customers;

[MutationType]
internal static partial class CustomerMutations
{
    /// <summary>
    /// RegisterCustomerHandler always succeeds and returns only the new customer id (see
    /// Bookshop.Application/Customers/RegisterCustomer.cs) - no <c>Result&lt;T&gt;</c>, so no
    /// typed errors are declared for this mutation. The payload reloads the full
    /// <see cref="CustomerReadModel"/> through the by-id DataLoader.
    /// </summary>
    public static async Task<CustomerReadModel> RegisterCustomerAsync(
        string name,
        string email,
        IMediator mediator,
        QueryContext<CustomerReadModel> query,
        ICustomerByIdDataLoader customerById,
        CancellationToken cancellationToken)
    {
        var customerId = await mediator.SendAsync(new RegisterCustomer(name, email), cancellationToken);

        return await customerById.With(query).LoadRequiredAsync(customerId, cancellationToken);
    }
}
