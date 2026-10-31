using MediatR;

namespace Shop.Ordering;

/// <summary>
/// Logs each request before invoking the next pipeline component.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Logs.HandlingMessage(logger, typeof(TRequest).Name);
        return await next();
    }
}

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Information, "Handling {Message}")]
    public static partial void HandlingMessage(ILogger logger, string message);
}
