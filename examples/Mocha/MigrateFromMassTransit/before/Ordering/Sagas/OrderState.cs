using MassTransit;

namespace Shop.Ordering;

/// <summary>
/// Stores the state of an order awaiting invoice confirmation or expiration.
/// </summary>
public sealed class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
    public Guid? TimeoutTokenId { get; set; }
    public uint RowVersion { get; set; }
}
