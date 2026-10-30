using Mocha;
using Mocha.Sagas;

namespace Shop.Contracts;

public sealed class InvoiceStatus
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
}
