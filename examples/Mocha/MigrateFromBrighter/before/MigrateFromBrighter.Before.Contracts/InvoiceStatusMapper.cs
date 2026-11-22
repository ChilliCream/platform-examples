using System.Text.Json;
using Paramore.Brighter;

namespace Shop.Contracts;

/// <summary>
/// Maps <see cref="InvoiceStatus"/> to and from a Brighter <see cref="Message"/>.
/// </summary>
public sealed class InvoiceStatusMapper : IAmAMessageMapper<InvoiceStatus>, IAmAMessageMapperAsync<InvoiceStatus>
{
    public IRequestContext? Context { get; set; }

    public Message MapToMessage(InvoiceStatus reply, Publication publication)
    {
        var header = new MessageHeader(
            messageId: reply.Id,
            topic: reply.SendersAddress.Topic,
            messageType: MessageType.MT_COMMAND,
            correlationId: reply.SendersAddress.CorrelationId);
        return new Message(header, new MessageBody(JsonSerializer.Serialize(new ReplyBody(reply.OrderId, reply.Status))));
    }

    public InvoiceStatus MapToRequest(Message message)
    {
        var body = JsonSerializer.Deserialize<ReplyBody>(message.Body.Value)!;
        return new InvoiceStatus(new ReplyAddress(message.Header.ReplyTo ?? RoutingKey.Empty, message.Header.CorrelationId))
        {
            OrderId = body.OrderId,
            Status = body.Status
        };
    }

    public Task<Message> MapToMessageAsync(
        InvoiceStatus request,
        Publication publication,
        CancellationToken cancellationToken = default)
        => Task.FromResult(MapToMessage(request, publication));

    public Task<InvoiceStatus> MapToRequestAsync(Message message, CancellationToken cancellationToken = default)
        => Task.FromResult(MapToRequest(message));

    /// <summary>
    /// Contains the serialized invoice status reply.
    /// </summary>
    /// <param name="OrderId">The order identifier.</param>
    /// <param name="Status">The invoice status, or <c>NotFound</c> when no invoice exists.</param>
    private sealed record ReplyBody(Guid OrderId, string Status);
}
