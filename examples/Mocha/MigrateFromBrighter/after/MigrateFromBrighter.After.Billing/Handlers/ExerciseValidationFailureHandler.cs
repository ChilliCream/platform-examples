using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Mocha;
using Shop.Contracts;

namespace Shop.Billing;

public sealed class ExerciseValidationFailureHandler(ILogger<ExerciseValidationFailureHandler> logger)
    : IEventRequestHandler<ExerciseValidationFailure>
{
    public ValueTask HandleAsync(ExerciseValidationFailure message, CancellationToken cancellationToken)
    {
        Logs.ValidationFailed(logger, message.OrderId);
        throw new ValidationException("Invalid billing request");
    }
}

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Information, "Validation failed for order {OrderId}")]
    public static partial void ValidationFailed(ILogger logger, Guid orderId);
}
