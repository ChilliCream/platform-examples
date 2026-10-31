using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Paramore.Brighter;
using Shop.Contracts;

namespace Shop.Billing;

public sealed class ExerciseValidationFailureHandler(ILogger<ExerciseValidationFailureHandler> logger)
    : RequestHandlerAsync<ExerciseValidationFailure>
{
    public override Task<ExerciseValidationFailure> HandleAsync(
        ExerciseValidationFailure message, CancellationToken cancellationToken = default)
    {
        // Validation failures are handled without retrying the message.
        Logs.ValidationFailed(logger, message.OrderId);
        throw new ValidationException("Invalid billing request");
    }
}

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Information, "Validation failed for order {OrderId}")]
    public static partial void ValidationFailed(ILogger logger, Guid orderId);
}
