using System.Diagnostics;
using Bookshop.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Bookshop.Application.Common;

/// <summary>
/// Records every domain event with application diagnostics until event subscribers are added.
/// </summary>
/// <remarks>
/// Mocha's mediator requires a registered notification pipeline. Because this example does not
/// yet have domain-event subscribers, the dispatcher records each event instead of publishing it.
/// It can be replaced by a mediator-backed dispatcher when subscribers are introduced.
/// </remarks>
public sealed partial class DomainEventDispatcher(ILogger<DomainEventDispatcher> logger)
    : IDomainEventDispatcher
{
    public const string ActivitySourceName = "Bookshop.Application";

    private static readonly ActivitySource s_activitySource = new(ActivitySourceName);

    public ValueTask DispatchAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            var eventType = domainEvent.GetType().FullName ?? domainEvent.GetType().Name;

            using var activity = s_activitySource.StartActivity(
                "domain-event.dispatch",
                ActivityKind.Internal);
            activity?.SetTag("domain.event.type", eventType);

            LogUnhandledDomainEvent(logger, eventType);
        }

        return ValueTask.CompletedTask;
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Domain event {DomainEventType} has no registered subscriber; dropping it.")]
    private static partial void LogUnhandledDomainEvent(ILogger logger, string domainEventType);
}
