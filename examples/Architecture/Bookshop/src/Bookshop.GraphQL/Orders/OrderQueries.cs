using Bookshop.Application.Orders.DataLoaders;
using Bookshop.Application.Orders.ReadModels;
using GreenDonut.Data;
using HotChocolate.Types.Relay;

namespace Bookshop.GraphQL.Orders;

[QueryType]
internal static partial class OrderQueries
{
    public static async Task<OrderReadModel?> GetOrderByIdAsync(
        [ID<OrderReadModel>] Guid id,
        QueryContext<OrderReadModel> query,
        IOrderByIdDataLoader orderById,
        CancellationToken cancellationToken)
        => await orderById.With(query).LoadAsync(id, cancellationToken);
}
