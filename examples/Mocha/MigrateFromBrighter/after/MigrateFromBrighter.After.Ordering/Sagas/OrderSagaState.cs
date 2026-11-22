using Mocha.Sagas;

namespace Shop.Ordering;

/// <summary>
/// Stores the order details and expiry for the saga.
/// </summary>
public sealed class OrderSagaState : SagaStateBase
{
    public string Product { get; set; } = string.Empty;
    public bool ConfirmInvoice { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
