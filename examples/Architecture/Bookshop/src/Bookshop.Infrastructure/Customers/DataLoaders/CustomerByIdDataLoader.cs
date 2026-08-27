using Bookshop.Application.Customers.DataLoaders;

namespace Bookshop.Infrastructure.Customers.DataLoaders;

/// <summary>
/// Binds the source-generated <c>CustomerByIdDataLoader</c> (from
/// <see cref="CustomerDataLoaders.GetCustomerByIdAsync"/>) to the Application-defined
/// <see cref="ICustomerByIdDataLoader"/> interface. See
/// <see cref="Bookshop.Infrastructure.Authors.DataLoaders.AuthorByIdDataLoader"/> for why this
/// partial binding exists.
/// </summary>
public sealed partial class CustomerByIdDataLoader : ICustomerByIdDataLoader;
