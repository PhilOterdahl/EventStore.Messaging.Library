namespace EventStore.Library.Messaging.Command;

internal interface ICommandPipelineHandler<TResponse>
{
    public Task<TResponse> Handle(ICommand<TResponse> command, CancellationToken cancellationToken = default);
}