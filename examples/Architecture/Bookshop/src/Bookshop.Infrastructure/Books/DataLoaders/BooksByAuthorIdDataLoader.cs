using Bookshop.Application.Books.DataLoaders;

namespace Bookshop.Infrastructure.Books.DataLoaders;

/// <summary>
/// Binds the source-generated <c>BooksByAuthorIdDataLoader</c> (from
/// <see cref="BookDataLoaders.GetBooksByAuthorIdAsync"/>) to the Application-defined
/// <see cref="IBooksByAuthorIdDataLoader"/> interface. See
/// <see cref="Bookshop.Infrastructure.Authors.DataLoaders.AuthorByIdDataLoader"/> for why this
/// partial binding exists.
/// </summary>
public sealed partial class BooksByAuthorIdDataLoader : IBooksByAuthorIdDataLoader;
