using MediatR;
using MediatR.Pipeline;

namespace Shop.Ordering;

// Run before the request pipeline begins.
public sealed class LoggingPreProcessor<TRequest>(ILogger<LoggingPreProcessor<TRequest>> logger)
    : IRequestPreProcessor<TRequest> where TRequest : notnull
{
    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        Logs.StartingMessage(logger, typeof(TRequest).Name);
        return Task.CompletedTask;
    }
}

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Information, "Starting {Message}")]
    public static partial void StartingMessage(ILogger logger, string message);
}
