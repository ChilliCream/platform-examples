using Mocha.Sagas;
using Shop.Contracts;

namespace Shop.Ordering;

public sealed record StartOrder
{
    public Guid OrderId { get; init; }
    public string Product { get; init; } = string.Empty;
    public bool ConfirmInvoice { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
}
