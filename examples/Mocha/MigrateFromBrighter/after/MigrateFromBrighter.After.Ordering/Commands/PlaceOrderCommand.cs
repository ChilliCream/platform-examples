using Microsoft.EntityFrameworkCore;
using Mocha;
using Mocha.Mediator;
using Shop.Contracts;

namespace Shop.Ordering;

public sealed record PlaceOrderCommand(Guid OrderId, string Product, bool ConfirmInvoice) : ICommand;

public sealed class PlaceOrderCommandHandler(
    OrderingDbContext db,
    IMessageBus bus,
    IConfiguration configuration,
    ILogger<PlaceOrderCommandHandler> logger) : ICommandHandler<PlaceOrderCommand>
{
    public async ValueTask HandleAsync(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        // The order and saga start share the EF Core transaction and outbox.
        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            db.Orders.Add(new Order { Id = command.OrderId, Product = command.Product });
            db.OrderProcesses.Add(new OrderProcess { OrderId = command.OrderId, Status = "Submitted" });
            await bus.SendAsync(new StartOrder
            {
                OrderId = command.OrderId,
                Product = command.Product,
                ConfirmInvoice = command.ConfirmInvoice,
                ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(configuration.GetValue<int>("Scenario:OrderTimeoutSeconds"))
            }, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });

        await bus.ScheduleSendAsync(
            new SendReminder { OrderId = command.OrderId },
            DateTimeOffset.UtcNow.AddSeconds(configuration.GetValue<int>("Scenario:ReminderDelaySeconds")),
            cancellationToken);
        Logs.OrderSubmitted(logger, command.OrderId);
    }
}

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Information, "Order {OrderId} submitted")]
    public static partial void OrderSubmitted(ILogger logger, Guid orderId);
}
