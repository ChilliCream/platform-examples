using Paramore.Brighter;

namespace Shop.Contracts;

public sealed class OrderPlaced : Event
{
    public OrderPlaced() : base(Id.Random()) { }

    public Guid OrderId { get; set; }
    public string Product { get; set; } = string.Empty;
    public bool ConfirmInvoice { get; set; }
}

public sealed class InvoiceConfirmed : Event
{
    public InvoiceConfirmed() : base(Id.Random()) { }

    public Guid OrderId { get; set; }
}

public sealed class ExpireOrder : Command
{
    public ExpireOrder() : base(Id.Random()) { }

    public Guid OrderId { get; set; }
}

public sealed class SendReminder : Command
{
    public SendReminder() : base(Id.Random()) { }

    public Guid OrderId { get; set; }
}

public sealed class ExerciseTransientFailure : Command
{
    public ExerciseTransientFailure() : base(Id.Random()) { }

    public Guid OrderId { get; set; }
}

public sealed class ExerciseValidationFailure : Command
{
    public ExerciseValidationFailure() : base(Id.Random()) { }

    public Guid OrderId { get; set; }
}

public sealed class CheckInvoiceStatus : Request
{
    public CheckInvoiceStatus() : base(new ReplyAddress(string.Empty, string.Empty)) { }

    public CheckInvoiceStatus(ReplyAddress address) : base(address) { }

    public Guid OrderId { get; set; }
}

public sealed class InvoiceStatus : Reply
{
    public InvoiceStatus(ReplyAddress address) : base(address) { }

    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
}
