namespace Shop.Contracts;

/// <summary>
/// Requests invoice processing with a specified failure mode.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
/// <param name="Failure">The failure mode: <c>transient</c>, <c>validation</c>, or another value for success.</param>
public sealed record ProcessInvoice(Guid OrderId, string Failure);
