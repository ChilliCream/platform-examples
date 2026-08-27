using Bookshop.Application.Customers.ReadModels;
using GreenDonut;

namespace Bookshop.Application.Customers.DataLoaders;

/// <summary>
/// Batches and caches customer-by-id lookups. A hand-written abstraction over a source-generated
/// fetch method implemented in Bookshop.Infrastructure, next to <c>BookshopContext</c>
/// (Infrastructure/Customers/DataLoaders) - Application depends only on this interface, never on
/// the generated class or on EF Core.
/// </summary>
public interface ICustomerByIdDataLoader : IDataLoader<Guid, CustomerReadModel>;
