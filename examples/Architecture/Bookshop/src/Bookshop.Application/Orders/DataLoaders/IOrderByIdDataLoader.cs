using Bookshop.Application.Orders.ReadModels;
using GreenDonut;

namespace Bookshop.Application.Orders.DataLoaders;

/// <summary>
/// Batches and caches order-by-id lookups. A hand-written abstraction over a source-generated
/// fetch method implemented in Bookshop.Infrastructure, next to <c>BookshopContext</c>
/// (Infrastructure/Orders/DataLoaders) - Application depends only on this interface, never on the
/// generated class or on EF Core.
/// </summary>
public interface IOrderByIdDataLoader : IDataLoader<Guid, OrderReadModel>;
