using Bookshop.Application.Authors.DataLoaders;
using Bookshop.Application.Books.DataLoaders;
using Bookshop.Application.Common;
using Bookshop.Application.Customers.DataLoaders;
using Bookshop.Application.Orders.DataLoaders;
using Bookshop.Domain.Authors;
using Bookshop.Domain.Books;
using Bookshop.Domain.Customers;
using Bookshop.Domain.Orders;
using Bookshop.Infrastructure.Authors.DataLoaders;
using Bookshop.Infrastructure.Authors.Persistence;
using Bookshop.Infrastructure.Books.DataLoaders;
using Bookshop.Infrastructure.Books.Persistence;
using Bookshop.Infrastructure.Common.Database;
using Bookshop.Infrastructure.Customers.DataLoaders;
using Bookshop.Infrastructure.Customers.Persistence;
using Bookshop.Infrastructure.Orders.DataLoaders;
using Bookshop.Infrastructure.Orders.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mocha.Mediator;

namespace Bookshop.Infrastructure;

/// <summary>
/// Wires the Bookshop infrastructure layer (database, repositories, DataLoader implementations,
/// and the catalog query handlers) into dependency injection.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddBookshopInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddBookshopDatabase(configuration);
        // Write side: repositories loading whole, tracked aggregate roots (Domain-defined
        // interfaces), plus the unit of work that commits them.
        services.AddScoped<IAuthorRepository, AuthorRepository>();
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Read side, DataLoaders: source-generated fetch classes (Authors|Books|Customers|Orders
        // /DataLoaders), registered by the generated AddBookshopDataLoaders() extension (from
        // [assembly: DataLoaderModule("BookshopDataLoaders")] in Properties/AssemblyInfo.cs) -
        // scoped, the same lifetime GreenDonut's DataLoaderServiceCollectionExtensions always
        // uses. Application depends only on the interfaces (IAuthorByIdDataLoader etc.), never
        // on these generated classes, so each interface is mapped here to the already-registered
        // concrete instance for the same scope (not a second, independently-constructed
        // registration) via GetRequiredService, preserving GreenDonut's per-request batch/cache
        // identity between the concrete-type and interface-type resolution paths.
        services.AddBookshopDataLoaders();
        services.AddScoped<IAuthorByIdDataLoader>(sp => sp.GetRequiredService<AuthorByIdDataLoader>());
        services.AddScoped<IAuthorsDataLoader>(sp => sp.GetRequiredService<AuthorsDataLoader>());
        services.AddScoped<IBookByIdDataLoader>(sp => sp.GetRequiredService<BookByIdDataLoader>());
        services.AddScoped<IBooksDataLoader>(sp => sp.GetRequiredService<BooksDataLoader>());
        services.AddScoped<IBooksByAuthorIdDataLoader>(sp => sp.GetRequiredService<BooksByAuthorIdDataLoader>());
        services.AddScoped<ICustomerByIdDataLoader>(sp => sp.GetRequiredService<CustomerByIdDataLoader>());
        services.AddScoped<IOrderByIdDataLoader>(sp => sp.GetRequiredService<OrderByIdDataLoader>());
        services.AddScoped<IOrdersByCustomerIdDataLoader>(sp => sp.GetRequiredService<OrdersByCustomerIdDataLoader>());

        // Commands execute atomically. The middleware compiles out of query and notification
        // pipelines, while UnitOfWork dispatches tracked domain events before SaveChanges.
        services.AddMediator().Use(TransactionMiddleware.Create());

        return services;
    }
}
