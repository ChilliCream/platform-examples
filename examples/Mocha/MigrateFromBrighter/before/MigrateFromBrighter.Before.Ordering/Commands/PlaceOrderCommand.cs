using Microsoft.EntityFrameworkCore;
using Paramore.Brighter;
using Shop.Contracts;

namespace Shop.Ordering;

public sealed class PlaceOrderCommand(Guid orderId, string product, bool confirmInvoice) : Command(Id.Random())
{
    public Guid OrderId { get; } = orderId;
    public string Product { get; } = product;
    public bool ConfirmInvoice { get; } = confirmInvoice;
}

public sealed class PlaceOrderCommandHandler(
    OrderingDbContext db,
    IAmATransactionConnectionProvider transactionProvider,
    IAmACommandProcessor processor,
    IConfiguration configuration,
    ILogger<PlaceOrderCommandHandler> logger) : RequestHandlerAsync<PlaceOrderCommand>
{
    public override async Task<PlaceOrderCommand> HandleAsync(
        PlaceOrderCommand command, CancellationToken cancellationToken = default)
    {
        // The order and outbox message share the EF Core transaction.
        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            db.Orders.Add(new Order
            {
                Id = command.OrderId,
                Product = command.Product
            });
            db.OrderProcesses.Add(new OrderProcess
            {
                OrderId = command.OrderId,
                Status = "Submitted"
            });
            await db.SaveChangesAsync(cancellationToken);

            await processor.DepositPostAsync(new OrderPlaced
            {
                OrderId = command.OrderId,
                Product = command.Product,
                ConfirmInvoice = command.ConfirmInvoice
            }, transactionProvider, cancellationToken: cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });

        await processor.PostAsync(
            TimeSpan.FromSeconds(configuration.GetValue<int>("Scenario:OrderTimeoutSeconds")),
            new ExpireOrder { OrderId = command.OrderId }, cancellationToken: cancellationToken);
        await processor.PostAsync(
            TimeSpan.FromSeconds(configuration.GetValue<int>("Scenario:ReminderDelaySeconds")),
            new SendReminder { OrderId = command.OrderId }, cancellationToken: cancellationToken);
        Logs.OrderSubmitted(logger, command.OrderId);

        return await base.HandleAsync(command, cancellationToken);
    }
}

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Information, "Order {OrderId} submitted")]
    public static partial void OrderSubmitted(ILogger logger, Guid orderId);
}
