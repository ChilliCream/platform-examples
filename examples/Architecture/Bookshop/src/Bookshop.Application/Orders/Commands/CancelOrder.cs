using Bookshop.Application.Common;
using Bookshop.Application.Common.Authorization;
using Bookshop.Domain.Orders;
using Mocha.Mediator;

namespace Bookshop.Application.Orders;

/// <summary>
/// Cancels a pending order. Fails when the order does not exist or is no longer pending.
/// </summary>
public sealed record CancelOrder(Guid OrderId) : ICommand<Result<Guid>>;

public sealed class CancelOrderHandler(
    IOrderRepository orders,
    IUnitOfWork unitOfWork,
    IAuthorizationService authorizationService)
    : ICommandHandler<CancelOrder, Result<Guid>>
{
    public async ValueTask<Result<Guid>> HandleAsync(CancelOrder command, CancellationToken cancellationToken)
    {
        await authorizationService.EnsurePermissionAsync(Permissions.Orders.Cancel, cancellationToken);

        var order = await orders.GetAsync(command.OrderId, cancellationToken);

        if (order is null)
        {
            return new NotFound($"Order '{command.OrderId}' was not found.");
        }

        if (order.Status != OrderStatus.Pending)
        {
            return new ValidationError(
                $"Only pending orders can be cancelled; order '{command.OrderId}' is {order.Status}.");
        }

        order.Cancel();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new Success<Guid>(order.Id);
    }
}
