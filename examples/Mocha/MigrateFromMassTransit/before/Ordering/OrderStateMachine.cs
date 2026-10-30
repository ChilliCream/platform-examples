using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Contracts;

namespace Shop.Ordering;

public sealed class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
    public Guid? TimeoutTokenId { get; set; }
    public uint RowVersion { get; set; }
}

public sealed class OrderStateMap : SagaClassMap<OrderState>
{
    protected override void Configure(EntityTypeBuilder<OrderState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64);
        entity.Property(x => x.RowVersion).HasColumnName("xmin").HasColumnType("xid").IsRowVersion();
    }
}

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
            .Then(c => logger.LogInformation("Order {OrderId} saga submitted", c.Saga.CorrelationId))
            .Schedule(Timeout, c => Task.FromResult(new OrderExpired(c.Saga.CorrelationId)))
            .TransitionTo(Submitted));

        During(Submitted,
            When(InvoiceConfirmed)
                .Unschedule(Timeout)
                .Then(c => logger.LogInformation("Order {OrderId} saga accepted", c.Saga.CorrelationId))
                .TransitionTo(Accepted),
            When(Timeout.Received)
                .Then(c => logger.LogInformation("Order {OrderId} saga expired", c.Saga.CorrelationId))
                .TransitionTo(Expired));
    }
}
