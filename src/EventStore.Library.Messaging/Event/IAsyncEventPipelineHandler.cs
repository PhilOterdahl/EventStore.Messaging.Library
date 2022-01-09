namespace EventStore.Library.Messaging.Event;

internal interface IAsyncEventPipelineHandler
{
    public Task Handle(IAsyncEvent @event, CancellationToken cancellationToken = default);
    public Task<IAsyncEvent> PreProcess(IAsyncEvent @event, CancellationToken cancellationToken = default);
}