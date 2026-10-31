using Mocha.Sagas;
using Shop.Contracts;

namespace Shop.Ordering;

// Publish the reply-producing event from the initial transition with the saga state.
/// <summary>
/// Tracks an order from submission to acceptance or expiry.
/// </summary>
public sealed class OrderSaga : Saga<OrderSagaState>
{
    protected override void Configure(ISagaDescriptor<OrderSagaState> descriptor)
    {
        descriptor.Initially()
            .OnRequest<StartOrder>()
            .StateFactory(message => new OrderSagaState
            {
                Id = message.OrderId,
                Product = message.Product,
                ConfirmInvoice = message.ConfirmInvoice,
                ExpiresAt = message.ExpiresAt
            })
            .Publish((_, state) => new ScheduleOrderExpiry(state.Id, state.ExpiresAt))
            .Publish((_, state) => new OrderPlaced
            {
                OrderId = state.Id,
                Product = state.Product,
                ConfirmInvoice = state.ConfirmInvoice
            })
            .TransitionTo("Submitted");

        descriptor.During("Submitted")
            .OnEvent<InvoiceConfirmed>()
            .Publish((_, state) => new OrderProcessChanged(state.Id, "Accepted"))
            .TransitionTo("Accepted");

        descriptor.During("Submitted")
            .OnRequest<ExpireOrder>()
            .Publish((_, state) => new OrderProcessChanged(state.Id, "Expired"))
            .TransitionTo("Expired");

        descriptor.Finally("Accepted");
        descriptor.Finally("Expired");
    }
}
