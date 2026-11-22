using Mocha.Sagas;

namespace Shop.Ordering;

/// <summary>
/// Stores the state of an <see cref="OrderSaga"/>.
/// </summary>
public class OrderState : SagaStateBase
{
    public string Product { get; set; } = "";
    public bool ConfirmInvoice { get; set; }
}
