using Microsoft.Extensions.DependencyInjection;

namespace EventStore.Library.Messaging.Command;

internal class AsyncCommandPipelineHandler<TAsyncCommand> : IAsyncCommandPipelineHandler where TAsyncCommand : IAsyncCommand
{
    private readonly IServiceProvider _serviceProvider;

    public AsyncCommandPipelineHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Handle(IAsyncCommand command, CancellationToken cancellationToken = default)
    {
        var handlers = _serviceProvider
            .GetServices<IAsyncCommandHandler<TAsyncCommand>>()
            .ToArray();

        if (handlers.Length > 1)
            throw new MultipleAsyncCommandHandlersRegisteredException(command.GetType().Name);

        if (!handlers.Any())
            throw new NoAsyncCommandHandlerRegisteredException(command.GetType().Name);

        var handler = handlers.First();

        await handler.Handle((TAsyncCommand)command, cancellationToken);
    }

    public async Task<IAsyncCommand> PreProcess(IAsyncCommand command, CancellationToken cancellationToken = default)
    {
        var messageHandler = (MessagePipelineHandler<TAsyncCommand>?)Activator.CreateInstance(
            typeof(MessagePipelineHandler<>).MakeGenericType(command.GetType()),
            _serviceProvider);

        if (messageHandler is null)
            throw new InvalidOperationException("MessagePipelineHandler can not be null");

        return await messageHandler
            .Handle((TAsyncCommand)command, cancellationToken)
            .ConfigureAwait(false);
    }
}