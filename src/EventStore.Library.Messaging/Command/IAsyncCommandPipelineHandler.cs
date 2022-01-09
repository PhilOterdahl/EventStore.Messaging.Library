namespace EventStore.Library.Messaging.Command;

internal interface IAsyncCommandPipelineHandler
{
    public Task Handle(IAsyncCommand command, CancellationToken cancellationToken = default);
    public Task<IAsyncCommand> PreProcess(IAsyncCommand command, CancellationToken cancellationToken = default);
}