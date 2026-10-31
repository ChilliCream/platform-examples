using System.ComponentModel.DataAnnotations;
using MassTransit;
using Shop.Contracts;

namespace Shop.Billing;

public sealed class ProcessInvoiceConsumer(
    InvoiceProcessor processor,
    ILogger<ProcessInvoiceConsumer> logger) : IConsumer<ProcessInvoice>
{
    public Task Consume(ConsumeContext<ProcessInvoice> context)
    {
        var message = context.Message;
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
        return Task.CompletedTask;
    }
}

internal static partial class Logs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} processing completed")]
    public static partial void ProcessingCompleted(ILogger logger, Guid orderId);
}
