using Bookshop.Application.Orders.DataLoaders;

namespace Bookshop.Infrastructure.Orders.DataLoaders;

/// <summary>
/// Binds the source-generated <c>OrdersByCustomerIdDataLoader</c> (from
/// <see cref="OrderDataLoaders.GetOrdersByCustomerIdAsync"/>) to the Application-defined
/// <see cref="IOrdersByCustomerIdDataLoader"/> interface. See
/// <see cref="Bookshop.Infrastructure.Authors.DataLoaders.AuthorByIdDataLoader"/> for why this
/// partial binding exists.
/// </summary>
public sealed partial class OrdersByCustomerIdDataLoader : IOrdersByCustomerIdDataLoader;
