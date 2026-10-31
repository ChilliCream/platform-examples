namespace Shop.Contracts;

public record FailOrder(Guid OrderId, string Failure);
