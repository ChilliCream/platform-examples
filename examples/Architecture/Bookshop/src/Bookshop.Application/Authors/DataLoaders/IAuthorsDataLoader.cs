using Bookshop.Application.Authors.ReadModels;
using Bookshop.Application.Common;
using GreenDonut;
using GreenDonut.Data;

namespace Bookshop.Application.Authors.DataLoaders;

/// <summary>
/// Loads and request-caches the paged author catalog. Paging and projection are DataLoader state.
/// </summary>
public interface IAuthorsDataLoader : IDataLoader<CatalogKey, Page<AuthorReadModel>>;
