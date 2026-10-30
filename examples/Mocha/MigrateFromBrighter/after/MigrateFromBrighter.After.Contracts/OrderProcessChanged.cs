namespace Shop.Contracts;

public sealed record OrderProcessChanged(Guid OrderId, string Status);
