using System.ComponentModel.DataAnnotations;
using Shop.Contracts;

namespace Shop.Ordering;

/// <summary>
/// Processes <see cref="FailOrder"/> messages with simulated failures.
/// </summary>
public static class FailOrderHandler
{
    // Validation errors are terminal; transient dependency errors use the retry policy.
    public static void Handle(FailOrder message, SimulatedOrderService service)
    {
        if (message.Failure == "validation")
        {
            throw new ValidationException("Order failed validation");
        }

        if (message.Failure == "transient")
        {
            service.Process(message.OrderId);
        }
    }
}
