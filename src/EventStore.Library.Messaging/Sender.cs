using EventStore.Library.Messaging.Command;
using EventStore.Library.Messaging.Event;
using EventStore.Library.Messaging.Query;

namespace EventStore.Library.Messaging;

internal class Sender : ISender
{
    private readonly IServiceProvider _serviceProvider;

    public Sender(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        var pipelineHandler = (ICommandPipelineHandler<TResponse>?)Activator.CreateInstance(
            typeof(CommandPipelineHandler<,>).MakeGenericType(command.GetType(),
                typeof(TResponse)),
            _serviceProvider
        );

        if (pipelineHandler is null)
            throw new InvalidOperationException("CommandPipelineHandler can not be null");

        return await pipelineHandler.Handle(command, cancellationToken);
    }

    public async Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        var pipelineHandler = (IQueryPipelineHandler<TResponse>?)Activator.CreateInstance(
            typeof(QueryPipelineHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse)),
            _serviceProvider
        );

        if (pipelineHandler is null)
            throw new InvalidOperationException("QueryPipelineHandler can not be null");

        return await pipelineHandler.Handle(query, cancellationToken);
    }

    public async Task Send(IAsyncCommand asyncCommand, CancellationToken cancellationToken = default)
    {
        var pipelineHandler = (IAsyncCommandPipelineHandler?)Activator.CreateInstance(
            typeof(AsyncCommandPipelineHandler<>).MakeGenericType(asyncCommand.GetType()),
            _serviceProvider);

        if (pipelineHandler is null)
            throw new InvalidOperationException("AsyncCommandPipelineHandler can not be null");

        await pipelineHandler.Handle(asyncCommand, cancellationToken);
    }

    public async Task Send(IAsyncEvent asyncEvent, CancellationToken cancellationToken = default)
    {
        var pipelineHandler = (IAsyncEventPipelineHandler?)Activator.CreateInstance(
            typeof(AsyncEventPipelineHandler<>).MakeGenericType(asyncEvent.GetType()),
            _serviceProvider);

        if (pipelineHandler is null)
            throw new InvalidOperationException("AsyncCommandPipelineHandler can not be null");

        await pipelineHandler.Handle(asyncEvent, cancellationToken);
    }
}