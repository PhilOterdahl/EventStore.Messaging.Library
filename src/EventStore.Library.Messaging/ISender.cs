using EventStore.Library.Messaging.Command;
using EventStore.Library.Messaging.Event;
using EventStore.Library.Messaging.Query;

namespace EventStore.Library.Messaging;

internal interface ISender
{
    public Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);

    public Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);

    public Task Send(IAsyncCommand command, CancellationToken cancellationToken = default);

    public Task Send(IAsyncEvent asyncEvent, CancellationToken cancellationToken = default);
}