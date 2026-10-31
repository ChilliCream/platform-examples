using System.Text.Json;
using Paramore.Brighter;

namespace Shop.Contracts;

/// <summary>
/// Maps <see cref="CheckInvoiceStatus"/> to and from a Brighter <see cref="Message"/>.
/// </summary>
public sealed class CheckInvoiceStatusMapper
    : IAmAMessageMapper<CheckInvoiceStatus>, IAmAMessageMapperAsync<CheckInvoiceStatus>
{
    public IRequestContext? Context { get; set; }

    public Message MapToMessage(CheckInvoiceStatus request, Publication publication)
    {
        if (publication.Topic is null)
        {
            throw new InvalidOperationException("Request publication requires a topic");
        }

        var header = new MessageHeader(
            messageId: request.Id,
            topic: publication.Topic,
            messageType: MessageType.MT_COMMAND,
            correlationId: request.ReplyAddress.CorrelationId,
            replyTo: request.ReplyAddress.Topic);
        return new Message(header, new MessageBody(JsonSerializer.Serialize(new RequestBody(request.OrderId))));
    }

    public CheckInvoiceStatus MapToRequest(Message message)
    {
        var body = JsonSerializer.Deserialize<RequestBody>(message.Body.Value)!;
        return new CheckInvoiceStatus(new ReplyAddress(message.Header.ReplyTo ?? RoutingKey.Empty, message.Header.CorrelationId))
        {
            OrderId = body.OrderId
        };
    }

    public Task<Message> MapToMessageAsync(
        CheckInvoiceStatus request, Publication publication, CancellationToken cancellationToken = default)
        => Task.FromResult(MapToMessage(request, publication));

    public Task<CheckInvoiceStatus> MapToRequestAsync(Message message, CancellationToken cancellationToken = default)
        => Task.FromResult(MapToRequest(message));

    /// <summary>
    /// Contains the serialized invoice status request.
    /// </summary>
    /// <param name="OrderId">The order identifier.</param>
    private sealed record RequestBody(Guid OrderId);
}
