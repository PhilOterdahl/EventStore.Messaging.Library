using EventStore.Library.Messaging.Command;
using EventStore.Library.Messaging.Event;
using EventStore.Library.Messaging.Query;

namespace EventStore.Library.Messaging;

internal class Dispatcher : IDispatcher
{
    private readonly ISender _sender;
    private readonly IPublisher _publisher;

    public Dispatcher(ISender sender, IPublisher publisher)
    {
        _sender = sender;
        _publisher = publisher;
    }

    public async Task Enqueue(IAsyncCommand asyncCommand, CancellationToken cancellationToken = default) => 
        await _publisher.Enqueue(asyncCommand, cancellationToken);

    public async Task Enqueue<TAsyncCommand>(
        TAsyncCommand[] asyncCommands, 
        int maxDegreeOfParallelism = 2,
        int batchSize = 1000,
        CancellationToken cancellationToken = default) where TAsyncCommand : IAsyncCommand =>
        await _publisher.Enqueue(asyncCommands, maxDegreeOfParallelism, batchSize, cancellationToken);

    public async Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default) =>
        await _sender.Send(command, cancellationToken);

    public async Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default) => 
        await _sender.Send(query, cancellationToken);

    public async Task Publish(IAsyncEvent asyncEvent, CancellationToken cancellationToken = default) => 
        await _publisher.Publish(asyncEvent, cancellationToken);
}