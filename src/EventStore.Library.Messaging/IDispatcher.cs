using EventStore.Library.Messaging.Command;
using EventStore.Library.Messaging.Event;
using EventStore.Library.Messaging.Query;

namespace EventStore.Library.Messaging;

public interface IDispatcher
{
    public Task Enqueue(IAsyncCommand asyncCommand, CancellationToken cancellationToken = default);

    public Task Enqueue<TAsyncCommand>(TAsyncCommand[] asyncCommands, int maxDegreeOfParallelism = 2, int batchSize = 1000, CancellationToken cancellationToken = default) where TAsyncCommand : IAsyncCommand;

    public Task<TResponse?> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);

    public Task<TResponse?> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);

    public Task Publish(IAsyncEvent asyncEvent, CancellationToken cancellationToken = default);
}