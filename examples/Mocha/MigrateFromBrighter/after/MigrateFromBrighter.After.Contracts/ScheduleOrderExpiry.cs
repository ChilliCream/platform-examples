namespace Shop.Contracts;

public sealed record ScheduleOrderExpiry(Guid OrderId, DateTimeOffset ExpiresAt);
