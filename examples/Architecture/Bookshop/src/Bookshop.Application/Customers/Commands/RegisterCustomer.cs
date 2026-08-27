using Bookshop.Application.Common;
using Bookshop.Application.Common.Authorization;
using Bookshop.Domain.Customers;
using Mocha.Mediator;

namespace Bookshop.Application.Customers;

/// <summary>
/// Registers a new customer.
/// </summary>
public sealed record RegisterCustomer(string Name, string Email) : ICommand<Guid>;

public sealed class RegisterCustomerHandler(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork,
    IAuthorizationService authorizationService)
    : ICommandHandler<RegisterCustomer, Guid>
{
    public async ValueTask<Guid> HandleAsync(RegisterCustomer command, CancellationToken cancellationToken)
    {
        await authorizationService.EnsurePermissionAsync(Permissions.Customers.Register, cancellationToken);

        var customer = Customer.Create(command.Name, command.Email);

        await customers.AddAsync(customer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return customer.Id;
    }
}
