using Microsoft.EntityFrameworkCore;
using Mocha;
using Shop.Contracts;

namespace Shop.Billing;

/// <summary>
/// Simulates a billing dependency that fails twice before succeeding.
/// </summary>
public sealed class ExerciseTransientFailureHandler(BillingDbContext db)
    : IEventRequestHandler<ExerciseTransientFailure>
{
    public async ValueTask HandleAsync(ExerciseTransientFailure message, CancellationToken cancellationToken)
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
    }
}
