using System.ComponentModel.DataAnnotations;
using Mocha;
using Shop.Contracts;

namespace Shop.Billing;

/// <summary>
/// Processes a <see cref="ProcessInvoice"/> request with the specified failure mode.
/// </summary>
public sealed class ProcessInvoiceConsumer(
    InvoiceProcessor processor,
    ILogger<ProcessInvoiceConsumer> logger) : IEventRequestHandler<ProcessInvoice>
{
    public ValueTask HandleAsync(ProcessInvoice message, CancellationToken cancellationToken)
    {
        if (message.Failure == "validation")
        {
            throw new ValidationException("Invoice is invalid");
        }

        // Simulate a briefly unavailable dependency without reading retry metadata.
        if (message.Failure == "transient")
        {
            processor.Process(message.OrderId);
        }

        Logs.ProcessingCompleted(logger, message.OrderId);
        return ValueTask.CompletedTask;
    }
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} processing completed")]
    public static partial void ProcessingCompleted(ILogger logger, Guid orderId);
}
