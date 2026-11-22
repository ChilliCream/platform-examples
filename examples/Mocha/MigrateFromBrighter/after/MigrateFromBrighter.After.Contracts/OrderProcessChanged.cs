namespace Shop.Contracts;

/// <summary>
/// Reports the terminal status of an order process.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
/// <param name="Status">The terminal order process status.</param>
public sealed record OrderProcessChanged(Guid OrderId, string Status);
