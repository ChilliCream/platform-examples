using Bookshop.Application.Customers.ReadModels;
using Bookshop.Infrastructure.Common.Database;
using GreenDonut;
using GreenDonut.Data;
using Microsoft.EntityFrameworkCore;

namespace Bookshop.Infrastructure.Customers.DataLoaders;

/// <summary>
/// Source-generated fetch method backing <see cref="Bookshop.Application.Customers.DataLoaders.ICustomerByIdDataLoader"/>.
/// The generator emits a matching <c>CustomerByIdDataLoader</c> class in this namespace; it is
/// bound to the Application interface via a partial class declaration in
/// <c>CustomerByIdDataLoader.cs</c> next to this file (interface generation is turned off for
/// this assembly, see Properties/AssemblyInfo.cs).
/// </summary>
internal static class CustomerDataLoaders
{
    [DataLoader]
    public static async Task<Dictionary<Guid, CustomerReadModel>> GetCustomerByIdAsync(
        IReadOnlyList<Guid> ids,
        QueryContext<CustomerReadModel> query,
        BookshopContext context,
        CancellationToken cancellationToken)
        => await context.Customers
            .AsNoTracking()
            .Where(customer => ids.Contains(customer.Id))
            .Select(ReadModelProjections.ToCustomerReadModel)
            .With(query.Include(customer => customer.Id))
            .ToDictionaryAsync(customer => customer.Id, cancellationToken);
}
