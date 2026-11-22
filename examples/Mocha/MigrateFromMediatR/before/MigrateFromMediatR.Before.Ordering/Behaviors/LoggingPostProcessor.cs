using MediatR;
using MediatR.Pipeline;

namespace Shop.Ordering;

/// <summary>
/// Logs when a request completes successfully.
/// </summary>
public sealed class LoggingPostProcessor<TRequest, TResponse>(ILogger<LoggingPostProcessor<TRequest, TResponse>> logger)
    : IRequestPostProcessor<TRequest, TResponse> where TRequest : notnull
{
    public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
    {
        Logs.FinishedMessage(logger, typeof(TRequest).Name);
        return Task.CompletedTask;
    }
}

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Information, "Finished {Message}")]
    public static partial void FinishedMessage(ILogger logger, string message);
}
