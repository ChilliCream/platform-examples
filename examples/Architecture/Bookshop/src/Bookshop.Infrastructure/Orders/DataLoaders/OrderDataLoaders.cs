using Bookshop.Application.Orders.ReadModels;
using Bookshop.Infrastructure.Common.Database;
using GreenDonut;
using GreenDonut.Data;
using Microsoft.EntityFrameworkCore;

namespace Bookshop.Infrastructure.Orders.DataLoaders;

/// <summary>
/// Source-generated fetch methods backing <see cref="Bookshop.Application.Orders.DataLoaders.IOrderByIdDataLoader"/>
/// and <see cref="Bookshop.Application.Orders.DataLoaders.IOrdersByCustomerIdDataLoader"/>. The
/// generator emits matching <c>OrderByIdDataLoader</c>/<c>OrdersByCustomerIdDataLoader</c>
/// classes in this namespace; each is bound to its Application interface via a partial class
/// declaration in its own file next to this one (interface generation is turned off for this
/// assembly, see Properties/AssemblyInfo.cs).
/// </summary>
internal static class OrderDataLoaders
{
    [DataLoader]
    public static async Task<Dictionary<Guid, OrderReadModel>> GetOrderByIdAsync(
        IReadOnlyList<Guid> ids,
        QueryContext<OrderReadModel> query,
        BookshopContext context,
        CancellationToken cancellationToken)
        => await context.Orders
            .AsNoTracking()
            .Where(order => ids.Contains(order.Id))
            .Select(ReadModelProjections.ToOrderReadModel)
            .With(query.Include(order => order.Id))
            .ToDictionaryAsync(order => order.Id, cancellationToken);

    // The stable order ends in the key so cursor pagination remains deterministic.
    [DataLoader]
    public static async Task<Dictionary<Guid, Page<OrderReadModel>>> GetOrdersByCustomerIdAsync(
        IReadOnlyList<Guid> customerIds,
        PagingArguments pagingArguments,
        QueryContext<OrderReadModel> query,
        BookshopContext context,
        CancellationToken cancellationToken)
        => await context.Orders
            .AsNoTracking()
            .Where(order => customerIds.Contains(order.CustomerId))
            .Select(ReadModelProjections.ToOrderReadModel)
            .OrderBy(order => order.Id)
            .With(query.Include(order => order.CustomerId))
            .ToBatchPageAsync(order => order.CustomerId, pagingArguments, cancellationToken);
}
