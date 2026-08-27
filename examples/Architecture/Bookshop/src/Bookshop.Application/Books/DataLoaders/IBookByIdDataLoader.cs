using Bookshop.Application.Books.ReadModels;
using GreenDonut;

namespace Bookshop.Application.Books.DataLoaders;

/// <summary>
/// Batches and caches book-by-id lookups. A hand-written abstraction over a source-generated
/// fetch method implemented in Bookshop.Infrastructure, next to <c>BookshopContext</c>
/// (Infrastructure/Books/DataLoaders) - Application depends only on this interface, never on the
/// generated class or on EF Core.
/// </summary>
public interface IBookByIdDataLoader : IDataLoader<Guid, BookReadModel>;
