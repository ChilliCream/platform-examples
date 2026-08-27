using Bookshop.Application.Orders.DataLoaders;

namespace Bookshop.Infrastructure.Orders.DataLoaders;

/// <summary>
/// Binds the source-generated <c>OrderByIdDataLoader</c> (from
/// <see cref="OrderDataLoaders.GetOrderByIdAsync"/>) to the Application-defined
/// <see cref="IOrderByIdDataLoader"/> interface. See
/// <see cref="Bookshop.Infrastructure.Authors.DataLoaders.AuthorByIdDataLoader"/> for why this
/// partial binding exists.
/// </summary>
public sealed partial class OrderByIdDataLoader : IOrderByIdDataLoader;
