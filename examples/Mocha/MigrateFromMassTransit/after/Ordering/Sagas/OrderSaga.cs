using Mocha;
using Mocha.Sagas;
using Shop.Contracts;

namespace Shop.Ordering;

// Correlate invoice confirmations and scheduled expirations to the submitted order.
public sealed class OrderSaga : Saga<OrderState>
{
    private const string Submitted = nameof(Submitted);
    private const string Accepted = nameof(Accepted);
    private const string Expired = nameof(Expired);

    protected override void Configure(ISagaDescriptor<OrderState> descriptor)
    {
        descriptor.Initially()
            .OnEvent<StartOrder>()
            .StateFactory(message => new OrderState { Id = message.OrderId })
            .TransitionTo(Submitted);

        descriptor.During(Submitted)
            .OnEvent<InvoiceConfirmed>()
            .TransitionTo(Accepted);

        descriptor.During(Submitted)
            .OnEvent<OrderExpired>()
            .TransitionTo(Expired);

        descriptor.Finally(Accepted);
        descriptor.Finally(Expired);
    }

    protected override async Task OnHandleTransitionAsync<TEvent>(OrderState state, TEvent message, IConsumeContext context)
    {
        var previousState = state.State;
        await base.OnHandleTransitionAsync(state, message, context);

        // Publish the reply-producing event in the initial saga transaction.
        if (message is StartOrder startOrder)
        {
            var bus = context.Services.GetRequiredService<IMessageBus>();
            await bus.PublishAsync(
                new OrderPlaced(startOrder.OrderId, startOrder.Amount, startOrder.ConfirmInvoice),
                context.CancellationToken);
            var timeout = context.Services.GetRequiredService<IConfiguration>()
                .GetValue<int>("Scenario:SagaTimeoutSeconds");
            var scheduled = await bus.SchedulePublishAsync(
                new OrderExpired(state.Id),
                DateTimeOffset.UtcNow.AddSeconds(timeout),
                context.CancellationToken);
            state.TimeoutToken = scheduled.Token;
        }

        // Retain the visible status after the saga reaches a final state.
        var db = context.Services.GetRequiredService<OrderingDbContext>();
        var status = await db.OrderStates.FindAsync([state.Id], context.CancellationToken);
        if (status is null)
        {
            status = new OrderStatus { CorrelationId = state.Id };
            db.OrderStates.Add(status);
        }

        status.CurrentState = state.State;
        await db.SaveChangesAsync(context.CancellationToken);

        var logger = context.Services.GetRequiredService<ILogger<OrderSaga>>();
        if (message is StartOrder)
        {
            Logs.SagaSubmitted(logger, state.Id);
        }
        else if (message is InvoiceConfirmed && state.State == Accepted)
        {
            Logs.SagaAccepted(logger, state.Id);
        }
        else if (message is OrderExpired && previousState == Submitted)
        {
            Logs.SagaExpired(logger, state.Id);
        }
    }
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} saga submitted")]
    public static partial void SagaSubmitted(ILogger logger, Guid orderId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} saga accepted")]
    public static partial void SagaAccepted(ILogger logger, Guid orderId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} saga expired")]
    public static partial void SagaExpired(ILogger logger, Guid orderId);
}
