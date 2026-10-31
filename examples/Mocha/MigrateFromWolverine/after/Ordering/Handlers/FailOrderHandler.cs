using System.ComponentModel.DataAnnotations;
using Mocha;
using Shop.Contracts;

namespace Shop.Ordering;

public class FailOrderHandler(SimulatedOrderService service) : IConsumer<FailOrder>
{
    // Validation errors are terminal; transient dependency errors use the retry policy.
    public ValueTask ConsumeAsync(IConsumeContext<FailOrder> context)
    {
        var message = context.Message;
        if (message.Failure == "validation")
        {
            throw new ValidationException("Order failed validation");
        }

        if (message.Failure == "transient")
        {
            service.Process(message.OrderId);
        }

        return ValueTask.CompletedTask;
    }
}
