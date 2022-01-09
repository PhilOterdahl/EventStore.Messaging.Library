using EventStore.Library.Messaging.Command;
using EventStore.Library.Messaging.Event;

namespace EventStore.Library.Messaging;

internal interface IPublisher
{
    public Task Enqueue(IAsyncCommand asyncCommand, CancellationToken cancellationToken = default);

    public Task Enqueue<TAsyncCommand>(TAsyncCommand[] asyncCommands, int maxDegreeOfParallelism = 2, int batchSize = 1000, CancellationToken cancellationToken = default) where TAsyncCommand : IAsyncCommand;

    public Task Publish(IAsyncEvent asyncEvent, CancellationToken cancellationToken = default);
}