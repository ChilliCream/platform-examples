using NServiceBus;

namespace Shop.Contracts;

public record OrderPlaced(Guid OrderId, bool ConfirmInvoice) : IEvent;
public record InvoiceConfirmed(Guid OrderId) : ICommand;
public record CheckInvoiceStatus(Guid OrderId) : IMessage;
public record InvoiceStatus(Guid OrderId, string Status) : IMessage;
public record SendReminder(Guid OrderId) : ICommand;
public record TryTransient(Guid Id) : ICommand;
public record TryInvalid(Guid Id) : ICommand;
