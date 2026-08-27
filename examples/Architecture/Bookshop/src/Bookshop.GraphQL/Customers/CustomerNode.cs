using Bookshop.Application.Customers.DataLoaders;
using Bookshop.Application.Customers.ReadModels;
using Bookshop.Application.Orders.DataLoaders;
using Bookshop.Application.Orders.ReadModels;
using GreenDonut.Data;
using HotChocolate.Types.Pagination;
using HotChocolate.Types.Relay;

namespace Bookshop.GraphQL.Customers;

/// <summary>
/// GraphQL shape for <see cref="CustomerReadModel"/>. No root query returns a customer directly
/// (only <c>registerCustomer</c> dispatches against customers, and books/orders reference a
/// customer only through the relay node interface or a nested association), so the only entry
/// points are the mutation payload, <see cref="OrderNode"/>'s <c>customer</c> field, and node
/// re-fetch.
/// </summary>
[ObjectType<CustomerReadModel>]
internal static partial class CustomerNode
{
    [ID<CustomerReadModel>]
    public static Guid GetId([Parent(requires: nameof(CustomerReadModel.Id))] CustomerReadModel customer)
        => customer.Id;

    static partial void Configure(IObjectTypeDescriptor<CustomerReadModel> descriptor)
        => descriptor.Name("Customer");

    [NodeResolver]
    public static async Task<CustomerReadModel?> GetCustomerByIdAsync(
        Guid id,
        QueryContext<CustomerReadModel> query,
        ICustomerByIdDataLoader customerById,
        CancellationToken cancellationToken)
        => await customerById.With(query).LoadAsync(id, cancellationToken);

    /// <summary>
    /// Paged association field for this customer's orders. Uses the batch-paging
    /// <c>IOrdersByCustomerIdDataLoader</c> (inline <c>OrderBy(Id)</c> per pagination.md, since
    /// this field has no client-controlled sorting per the R3b ruling: no <c>UseSorting</c> on
    /// this field unless paired with a <c>SortDefinition</c> default-order change in
    /// Bookshop.Application).
    /// </summary>
    [UseConnection]
    public static async Task<PageConnection<OrderReadModel>> GetOrdersAsync(
        [Parent(requires: nameof(CustomerReadModel.Id))] CustomerReadModel customer,
        PagingArguments pagingArguments,
        QueryContext<OrderReadModel> query,
        IOrdersByCustomerIdDataLoader ordersByCustomerId,
        CancellationToken cancellationToken)
        => await ordersByCustomerId.With(pagingArguments, query).LoadAsync(customer.Id, cancellationToken)
            ?? Page<OrderReadModel>.Empty;
}
