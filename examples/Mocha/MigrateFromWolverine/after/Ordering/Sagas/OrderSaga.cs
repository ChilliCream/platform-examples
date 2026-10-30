using Mocha;
using Mocha.Sagas;
using Shop.Contracts;

namespace Shop.Ordering;

// Mocha persists the state and correlates later messages by the saga Id.
public class OrderSaga : Saga<OrderState>
{
    public static TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    protected override void Configure(ISagaDescriptor<OrderState> descriptor)
    {
        descriptor.Timeout(Timeout);

        // The initial transition publishes the reply-producing event with the saga state.
        descriptor.Initially()
            .OnEvent<OrderSubmitted>()
            .StateFactory(message => new OrderState
            {
                Id = message.Id,
                Product = message.Product,
                ConfirmInvoice = message.ConfirmInvoice
            })
            .TransitionTo("Submitted")
            .Publish((_, state) => new OrderPlaced(state.Id, state.Product, state.ConfirmInvoice));

        descriptor.During("Submitted")
            .OnEvent<InvoiceConfirmed>()
            .TransitionTo("Accepted");

        descriptor.During("Submitted")
            .OnTimeout()
            .TransitionTo(StateNames.TimedOut);

        descriptor.Finally("Accepted");
    }

    // Update the business order in the same scoped transaction as the saga transition.
    protected override async Task OnEnterStateAsync(
        OrderState state,
        Mocha.Sagas.SagaState nextState,
        IConsumeContext context)
    {
        var logger = context.Services.GetRequiredService<ILogger<OrderSaga>>();
        if (nextState.State == "Submitted")
        {
            Logs.SagaSubmitted(logger, state.Id);
        }
        else if (nextState.State is "Accepted" or StateNames.TimedOut)
        {
            var db = context.Services.GetRequiredService<OrderingDb>();
            var order = await db.Orders.FindAsync([state.Id], context.CancellationToken);
            if (nextState.State == "Accepted")
            {
                if (order is not null)
                {
                    order.Status = "Accepted";
                }

                Logs.OrderAccepted(logger, state.Id);
            }
            else
            {
                if (order is not null)
                {
                    order.Status = "Expired";
                }

                Logs.OrderExpired(logger, state.Id);
            }

            await db.SaveChangesAsync(context.CancellationToken);
        }

        await base.OnEnterStateAsync(state, nextState, context);
    }
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} saga submitted")]
    public static partial void SagaSubmitted(ILogger logger, Guid orderId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} accepted")]
    public static partial void OrderAccepted(ILogger logger, Guid orderId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} expired")]
    public static partial void OrderExpired(ILogger logger, Guid orderId);
}
