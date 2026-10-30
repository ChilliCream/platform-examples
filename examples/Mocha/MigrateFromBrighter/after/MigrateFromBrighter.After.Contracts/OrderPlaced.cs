using Mocha;
using Mocha.Sagas;

namespace Shop.Contracts;

public sealed class OrderPlaced
{
    public Guid OrderId { get; set; }
    public string Product { get; set; } = string.Empty;
    public bool ConfirmInvoice { get; set; }
}
