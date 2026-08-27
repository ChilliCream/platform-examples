using Bookshop.Application.Authors.ReadModels;
using GreenDonut;

namespace Bookshop.Application.Authors.DataLoaders;

/// <summary>
/// Batches and caches author-by-id lookups. A hand-written abstraction over a source-generated
/// fetch method implemented in Bookshop.Infrastructure, next to <c>BookshopContext</c>
/// (Infrastructure/Authors/DataLoaders) - Application depends only on this interface, never on
/// the generated class or on EF Core.
/// </summary>
public interface IAuthorByIdDataLoader : IDataLoader<Guid, AuthorReadModel>;
