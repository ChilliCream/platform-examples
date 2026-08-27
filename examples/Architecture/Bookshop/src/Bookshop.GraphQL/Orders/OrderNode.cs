using Bookshop.Application.Customers.DataLoaders;
using Bookshop.Application.Customers.ReadModels;
using Bookshop.Application.Orders.DataLoaders;
using Bookshop.Application.Orders.ReadModels;
using GreenDonut.Data;
using HotChocolate.Types.Relay;

namespace Bookshop.GraphQL.Orders;

/// <summary>
/// GraphQL shape for <see cref="OrderReadModel"/>. <c>Lines</c> needs no resolver: it is already loaded as
/// part of the DataLoader fetch (bounded per order, unlike the paged
/// <c>Author.books</c>/<c>Customer.orders</c> associations), so it is exposed as a plain list
/// field straight off the read model.
/// </summary>
[ObjectType<OrderReadModel>]
internal static partial class OrderNode
{
    [ID<OrderReadModel>]
    public static Guid GetId([Parent(requires: nameof(OrderReadModel.Id))] OrderReadModel order) => order.Id;

    static partial void Configure(IObjectTypeDescriptor<OrderReadModel> descriptor)
        => descriptor.Name("Order");

    /// <summary>
    /// Customer association, resolved through the by-id DataLoader. Every order has a customer
    /// (<see cref="OrderReadModel.CustomerId"/> is required), so this loads non-null.
    /// </summary>
    [BindMember(nameof(OrderReadModel.CustomerId))]
    public static async Task<CustomerReadModel> GetCustomerAsync(
        [Parent(requires: nameof(OrderReadModel.CustomerId))] OrderReadModel order,
        QueryContext<CustomerReadModel> query,
        ICustomerByIdDataLoader customerById,
        CancellationToken cancellationToken)
        => await customerById.With(query).LoadRequiredAsync(order.CustomerId, cancellationToken);

    /// <summary>
    /// Relay node resolver for <see cref="OrderReadModel"/>, backed by the same
    /// <c>IOrderByIdDataLoader</c> the <c>orderById</c> root field uses.
    /// </summary>
    [NodeResolver]
    public static async Task<OrderReadModel?> GetOrderByIdAsync(
        Guid id,
        QueryContext<OrderReadModel> query,
        IOrderByIdDataLoader orderById,
        CancellationToken cancellationToken)
        => await orderById.With(query).LoadAsync(id, cancellationToken);
}
