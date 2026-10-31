using Wolverine;

namespace Shop.Ordering;

public record OrderExpired(Guid Id, TimeSpan Delay) : TimeoutMessage(Delay);
