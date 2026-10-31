using Mocha.Sagas;

namespace Shop.Ordering;

public class OrderState : SagaStateBase
{
    public string Product { get; set; } = "";
    public bool ConfirmInvoice { get; set; }
}
