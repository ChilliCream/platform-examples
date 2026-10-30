using Microsoft.EntityFrameworkCore;
using Mocha;
using Mocha.Sagas;
using Shop.Contracts;

namespace Shop.Ordering;

public class OrderSagaData : SagaStateBase;

public class OrderSaga : Saga<OrderSagaData>
{
    private const string Submitted = nameof(Submitted);
    private const string Accepted = nameof(Accepted);

    public static TimeSpan Timeout { get; set; }

    protected override void Configure(ISagaDescriptor<OrderSagaData> descriptor)
    {
        descriptor.Timeout(Timeout);
        descriptor.Initially()
            .OnEvent<OrderPlaced>()
            .StateFactory(message => new OrderSagaData { Id = message.OrderId })
            .TransitionTo(Submitted);
        descriptor.During(Submitted)
            .OnSend<InvoiceConfirmed>()
            .TransitionTo(Accepted);
        descriptor.During(Submitted)
            .OnTimeout()
            .TransitionTo(StateNames.TimedOut);
        descriptor.Finally(Accepted);
    }

    protected override async Task OnHandleTransitionAsync<TEvent>(
        OrderSagaData state,
        TEvent message,
        IConsumeContext context)
    {
        await base.OnHandleTransitionAsync(state, message, context);
        var logger = context.Services.GetRequiredService<ILogger<OrderSaga>>();
        if (message is OrderPlaced)
        {
            logger.LogInformation("Order {OrderId} saga submitted", state.Id);
        }
        else if (message is InvoiceConfirmed or SagaTimedOutEvent)
        {
            var status = message is InvoiceConfirmed ? "Accepted" : "Expired";
            var db = context.Services.GetRequiredService<OrderingDbContext>();
            await db.Orders.Where(x => x.Id == state.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, status), context.CancellationToken);
            if (message is InvoiceConfirmed)
            {
                logger.LogInformation("Order {OrderId} accepted", state.Id);
            }
            else
            {
                logger.LogInformation("Order {OrderId} expired", state.Id);
            }
        }
    }
}
