using Bookshop.Application.Books.ReadModels;
using GreenDonut;
using GreenDonut.Data;

namespace Bookshop.Application.Books.DataLoaders;

/// <summary>
/// Batches and paginates the <c>Author.books</c> relation. A hand-written abstraction over a
/// source-generated batch-paging fetch method implemented in Bookshop.Infrastructure, next to
/// <c>BookshopContext</c> (Infrastructure/Books/DataLoaders) - Application depends only on this
/// interface, never on the generated class or on EF Core.
/// </summary>
public interface IBooksByAuthorIdDataLoader : IDataLoader<Guid, Page<BookReadModel>>;
