using MassTransit;
using Shop.Contracts;

namespace Shop.Ordering;

// Correlate invoice confirmations and scheduled expirations to the submitted order.
public sealed class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State Submitted { get; private set; } = null!;
    public State Accepted { get; private set; } = null!;
    public State Expired { get; private set; } = null!;
    public Event<OrderPlaced> OrderPlaced { get; private set; } = null!;
    public Event<InvoiceConfirmed> InvoiceConfirmed { get; private set; } = null!;
    public Schedule<OrderState, OrderExpired> Timeout { get; private set; } = null!;

    public OrderStateMachine(IConfiguration configuration, ILogger<OrderStateMachine> logger)
    {
        InstanceState(x => x.CurrentState);
        Event(() => OrderPlaced, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => InvoiceConfirmed, x => x.CorrelateById(m => m.Message.OrderId));
        Schedule(() => Timeout, x => x.TimeoutTokenId, x =>
        {
            x.Delay = TimeSpan.FromSeconds(configuration.GetValue<int>("Scenario:SagaTimeoutSeconds"));
            x.Received = e => e.CorrelateById(m => m.Message.OrderId);
        });

        Initially(When(OrderPlaced)
            .Then(c => Logs.SagaSubmitted(logger, c.Saga.CorrelationId))
            .Schedule(Timeout, c => Task.FromResult(new OrderExpired(c.Saga.CorrelationId)))
            .TransitionTo(Submitted));

        During(Submitted,
            When(InvoiceConfirmed)
                .Unschedule(Timeout)
                .Then(c => Logs.SagaAccepted(logger, c.Saga.CorrelationId))
                .TransitionTo(Accepted),
            When(Timeout.Received)
                .Then(c => Logs.SagaExpired(logger, c.Saga.CorrelationId))
                .TransitionTo(Expired));
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
