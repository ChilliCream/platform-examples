using Mocha.Sagas;

namespace Shop.Ordering;

/// <summary>
/// Stores the state of an order awaiting invoice confirmation or expiration.
/// </summary>
public sealed class OrderState : SagaStateBase;
