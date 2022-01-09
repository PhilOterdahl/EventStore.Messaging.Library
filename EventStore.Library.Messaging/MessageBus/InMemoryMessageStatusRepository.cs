using System.Collections.Concurrent;
using EventStore.Library.Core.Event;

namespace EventStore.Library.Messaging.MessageBus;

internal record MessageId(string Handler, Guid Id);

internal class InMemoryMessageStatusRepository : IMessageStatusRepository
{
    private readonly ConcurrentDictionary<MessageId, MessageStatus> _messages = new();

    public Task<MessageStatus?> TryLoadMessageStatus(IEventStoreEvent message, string handler, string streamId,
        CancellationToken cancellationToken = default)
    {
        var id = new MessageId(handler, message.Id.ToGuid());
        return Task.FromResult(
            _messages.TryGetValue(id, out var messageStatus)
                ? messageStatus
                : null
        );
    }

    public Task<MessageStatus> ProcessingMessage(
        Guid id, 
        string handler,
        string streamId, 
        long messageNumber,
        CancellationToken cancellationToken = default)
    {
        var messageId = new MessageId(handler, id);
        var messageStatus = new MessageStatus
        {
            Handler = handler,
            Id = id,
            Number = messageNumber,
            Status = Status.Processing,
            Stream = streamId
        };

        _messages[messageId] = messageStatus;

        return Task.FromResult(messageStatus);
    }

    public Task SetMessageProcessed(MessageStatus messageStatus, CancellationToken cancellationToken = default)
    {
        var messageId = new MessageId(messageStatus.Handler, messageStatus.Id);
        var message = _messages[messageId];
        message.Status = Status.Processed;

        return Task.CompletedTask;
    }

    public Task ParkMessage(MessageStatus messageStatus, CancellationToken cancellationToken = default)
    {
        var messageId = new MessageId(messageStatus.Handler, messageStatus.Id);
        var message = _messages[messageId];
        message.Status = Status.Parked;

        return Task.CompletedTask;
    }

    public Task RetryMessage(MessageStatus messageStatus, CancellationToken cancellationToken = default)
    {
        var messageId = new MessageId(messageStatus.Handler, messageStatus.Id);
        var message = _messages[messageId];
        message.Status = Status.Retrying;

        return Task.CompletedTask;
    }
}