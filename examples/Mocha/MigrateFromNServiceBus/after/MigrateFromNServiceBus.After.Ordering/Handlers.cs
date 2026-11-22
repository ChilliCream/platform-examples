using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using Mocha;
using Shop.Contracts;

namespace Shop.Ordering;

public class ReminderHandler(ILogger<ReminderHandler> logger) : IEventRequestHandler<SendReminder>
{
    public ValueTask HandleAsync(SendReminder message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Reminder for order {OrderId}", message.OrderId);
        return ValueTask.CompletedTask;
    }
}

public class TransientHandler(ILogger<TransientHandler> logger) : IEventRequestHandler<TryTransient>
{
    private static readonly ConcurrentDictionary<Guid, int> Attempts = new();

    public ValueTask HandleAsync(TryTransient message, CancellationToken cancellationToken)
    {
        var attempt = Attempts.AddOrUpdate(message.Id, 1, (_, count) => count + 1);
        logger.LogInformation("Transient message {Id} attempt {Attempt}", message.Id, attempt);
        if (attempt < 3)
        {
            throw new InvalidOperationException("Temporary failure");
        }
        Attempts.TryRemove(message.Id, out _);
        return ValueTask.CompletedTask;
    }
}

public class ValidationHandler(ILogger<ValidationHandler> logger) : IEventRequestHandler<TryInvalid>
{
    public ValueTask HandleAsync(TryInvalid message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Validation failed for message {Id}", message.Id);
        throw new ValidationException("Invalid message");
    }
}
