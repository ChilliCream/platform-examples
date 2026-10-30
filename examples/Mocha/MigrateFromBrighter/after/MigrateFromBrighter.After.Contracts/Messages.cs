using Mocha;

namespace Shop.Contracts;

public sealed class OrderPlaced
{
    public Guid OrderId { get; set; }
    public string Product { get; set; } = string.Empty;
    public bool ConfirmInvoice { get; set; }
}

public sealed class InvoiceConfirmed
{
    public Guid OrderId { get; set; }
}

public sealed class ExpireOrder
{
    public Guid OrderId { get; set; }
}

public sealed class SendReminder
{
    public Guid OrderId { get; set; }
}

public sealed class ExerciseTransientFailure
{
    public Guid OrderId { get; set; }
}

public sealed class ExerciseValidationFailure
{
    public Guid OrderId { get; set; }
}

public sealed class CheckInvoiceStatus : IEventRequest<InvoiceStatus>
{
    public Guid OrderId { get; set; }
}

public sealed class InvoiceStatus
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
}
