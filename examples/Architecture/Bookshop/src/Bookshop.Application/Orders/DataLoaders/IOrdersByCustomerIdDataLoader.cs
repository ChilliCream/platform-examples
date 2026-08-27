using Bookshop.Application.Orders.ReadModels;
using GreenDonut;
using GreenDonut.Data;

namespace Bookshop.Application.Orders.DataLoaders;

/// <summary>
/// Batches and paginates the <c>Customer.orders</c> relation. A hand-written abstraction over a
/// source-generated batch-paging fetch method implemented in Bookshop.Infrastructure, next to
/// <c>BookshopContext</c> (Infrastructure/Orders/DataLoaders) - Application depends only on this
/// interface, never on the generated class or on EF Core.
/// </summary>
public interface IOrdersByCustomerIdDataLoader : IDataLoader<Guid, Page<OrderReadModel>>;
