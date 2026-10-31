using Microsoft.EntityFrameworkCore;
using Paramore.Brighter;
using Paramore.Brighter.Defer.Attributes;
using Paramore.Brighter.Policies.Attributes;
using Shop.Contracts;

namespace Shop.Billing;

/// <summary>
/// Simulates a billing dependency that fails twice before succeeding.
/// </summary>
public sealed class ExerciseTransientFailureHandler(BillingDbContext db)
    : RequestHandlerAsync<ExerciseTransientFailure>
{
    [DeferMessageOnErrorAsync(step: 0, delayMilliseconds: 1000)]
    [UseResiliencePipelineAsync("TransientRetry", step: 1)]
    public override async Task<ExerciseTransientFailure> HandleAsync(
        ExerciseTransientFailure message, CancellationToken cancellationToken = default)
    {
        // Simulate a dependency that recovers after two failed attempts.
        var attempt = await db.Attempts.FindAsync([message.OrderId], cancellationToken);
        if (attempt is null)
        {
            attempt = new Attempt { OrderId = message.OrderId };
            db.Attempts.Add(attempt);
        }

        attempt.Count++;
        await db.SaveChangesAsync(cancellationToken);

        if (attempt.Count < 3)
        {
            throw new TransientException("Temporary billing failure");
        }

        return await base.HandleAsync(message, cancellationToken);
    }
}
