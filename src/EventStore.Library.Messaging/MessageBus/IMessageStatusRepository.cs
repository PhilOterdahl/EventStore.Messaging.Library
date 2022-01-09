using EventStore.Library.Core.Event;

namespace EventStore.Library.Messaging.MessageBus;

internal interface IMessageStatusRepository
{
    public Task<MessageStatus?> TryLoadMessageStatus(
        IEventStoreEvent message,
        string handler,
        string streamId,
        CancellationToken cancellationToken = default);

    public Task<MessageStatus?> ProcessingMessage(
        Guid id,
        string handler,
        string streamId,
        long messageNumber,
        CancellationToken cancellationToken = default);

    public Task SetMessageProcessed(
        MessageStatus messageStatus,
        CancellationToken cancellationToken = default);

    public Task ParkMessage(
        MessageStatus messageStatus,
        CancellationToken cancellationToken = default);

    public Task RetryMessage(
        MessageStatus messageStatus,
        CancellationToken cancellationToken = default);
}