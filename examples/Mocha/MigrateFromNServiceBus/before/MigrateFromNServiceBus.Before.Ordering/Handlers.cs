using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using NServiceBus;
using Shop.Contracts;

namespace Shop.Ordering;

public class ReminderHandler(ILogger<ReminderHandler> logger) : IHandleMessages<SendReminder>
{
    public Task Handle(SendReminder message, IMessageHandlerContext context)
    {
        logger.LogInformation("Reminder for order {OrderId}", message.OrderId);
        return Task.CompletedTask;
    }
}

public class TransientHandler(ILogger<TransientHandler> logger) : IHandleMessages<TryTransient>
{
    private static readonly ConcurrentDictionary<Guid, int> Attempts = new();

    public Task Handle(TryTransient message, IMessageHandlerContext context)
    {
        var attempt = Attempts.AddOrUpdate(message.Id, 1, (_, count) => count + 1);
        logger.LogInformation("Transient message {Id} attempt {Attempt}", message.Id, attempt);
        if (attempt < 3)
        {
            throw new InvalidOperationException("Temporary failure");
        }
        Attempts.TryRemove(message.Id, out _);
        return Task.CompletedTask;
    }
}

public class ValidationHandler(ILogger<ValidationHandler> logger) : IHandleMessages<TryInvalid>
{
    public Task Handle(TryInvalid message, IMessageHandlerContext context)
    {
        logger.LogInformation("Validation failed for message {Id}", message.Id);
        throw new ValidationException("Invalid message");
    }
}
