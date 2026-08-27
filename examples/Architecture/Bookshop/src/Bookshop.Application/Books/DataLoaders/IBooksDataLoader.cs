using Bookshop.Application.Books.ReadModels;
using Bookshop.Application.Common;
using GreenDonut;
using GreenDonut.Data;

namespace Bookshop.Application.Books.DataLoaders;

/// <summary>
/// Loads and request-caches the paged book catalog. Paging and projection are DataLoader state.
/// </summary>
public interface IBooksDataLoader : IDataLoader<CatalogKey, Page<BookReadModel>>;
