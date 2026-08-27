using Bookshop.Application.Books.DataLoaders;

namespace Bookshop.Infrastructure.Books.DataLoaders;

/// <summary>
/// Binds the source-generated <c>BookByIdDataLoader</c> (from
/// <see cref="BookDataLoaders.GetBookByIdAsync"/>) to the Application-defined
/// <see cref="IBookByIdDataLoader"/> interface. See
/// <see cref="Bookshop.Infrastructure.Authors.DataLoaders.AuthorByIdDataLoader"/> for why this
/// partial binding exists.
/// </summary>
public sealed partial class BookByIdDataLoader : IBookByIdDataLoader;
