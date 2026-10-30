using Mocha;
using Mocha.Sagas;
using Shop.Contracts;

namespace Shop.Ordering;

public sealed class OrderState : SagaStateBase;

public sealed class OrderSaga : Saga<OrderState>
{
    private const string Submitted = nameof(Submitted);
    private const string Accepted = nameof(Accepted);
    private const string Expired = nameof(Expired);

    protected override void Configure(ISagaDescriptor<OrderState> descriptor)
    {
        descriptor.Initially()
            .OnEvent<OrderPlaced>()
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
        if (message is OrderPlaced)
        {
            var timeout = context.Services.GetRequiredService<IConfiguration>()
                .GetValue<int>("Scenario:SagaTimeoutSeconds");
            var scheduled = await context.Services.GetRequiredService<IMessageBus>().SchedulePublishAsync(
                new OrderExpired(state.Id),
                DateTimeOffset.UtcNow.AddSeconds(timeout),
                context.CancellationToken);
            state.TimeoutToken = scheduled.Token;
        }
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
        if (message is OrderPlaced)
        {
            logger.LogInformation("Order {OrderId} saga submitted", state.Id);
        }
        else if (message is InvoiceConfirmed && state.State == Accepted)
        {
            logger.LogInformation("Order {OrderId} saga accepted", state.Id);
        }
        else if (message is OrderExpired && previousState == Submitted)
        {
            logger.LogInformation("Order {OrderId} saga expired", state.Id);
        }
    }
}
