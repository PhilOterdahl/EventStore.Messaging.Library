using Microsoft.Extensions.DependencyInjection;

namespace EventStore.Library.Messaging.Command;

internal class CommandPipelineHandler<TCommand, TResponse> : ICommandPipelineHandler<TResponse> where TCommand : ICommand<TResponse>
{
    private readonly IServiceProvider _serviceProvider;

    public CommandPipelineHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Handle(
        ICommand<TResponse> command,
        CancellationToken cancellationToken = default)
    {
        var messagePipelineHandler = (MessagePipelineHandler<TCommand>?)Activator.CreateInstance(
            typeof(MessagePipelineHandler<>).MakeGenericType(command.GetType()),
            _serviceProvider
        );

        if (messagePipelineHandler is null)
            throw new InvalidOperationException("MessagePipelineHandler can not be null");

        command = await messagePipelineHandler.Handle((TCommand)command, cancellationToken);

        var handlers = _serviceProvider
            .GetServices<ICommandHandler<TCommand, TResponse>>()
            .ToArray();

        if (handlers.Length > 1)
            throw new MultipleCommandHandlersRegisteredException(command.GetType().Name);

        if (!handlers.Any())
            throw new NoCommandHandlerRegisteredException(command.GetType().Name);

        var handler = handlers.First();

        return await handler
            .Handle((TCommand)command, cancellationToken)
            .ConfigureAwait(false);
    }
}