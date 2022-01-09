using Microsoft.Extensions.DependencyInjection;

namespace EventStore.Library.Messaging.Query;

internal class QueryPipelineHandler<TQuery, TResponse> : IQueryPipelineHandler<TResponse> where TQuery : IQuery<TResponse>
{
    private readonly IQueryPipelineBehavior<TQuery, TResponse>[] _behaviors;
    private readonly IServiceProvider _serviceProvider;

    public QueryPipelineHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _behaviors = serviceProvider
            .GetServices<IQueryPipelineBehavior<TQuery, TResponse>>()
            .ToArray();
    }

    public async Task<TResponse> Handle(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default)
    {
        var messagePipelineHandler = (MessagePipelineHandler<TQuery>?)Activator.CreateInstance(
            typeof(MessagePipelineHandler<>).MakeGenericType(query.GetType()),
            _serviceProvider
        );

        if (messagePipelineHandler is null)
            throw new InvalidOperationException("MessagePipelineHandler can not be null");

        query = await messagePipelineHandler.Handle((TQuery)query, cancellationToken);

        var handlers = _serviceProvider
            .GetServices<IQueryHandler<TQuery, TResponse>>()
            .ToArray();

        if (handlers.Length > 1)
            throw new MultipleQueryHandlersRegisteredException(query.GetType().Name);

        if (!handlers.Any())
            throw new NoQueryHandlerRegisteredException(query.GetType().Name);

        var handler = handlers.First();

        Task<TResponse> HandleTask() => handler.Handle((TQuery)query, cancellationToken);

        return await _behaviors
            .Reverse()
            .Aggregate(
                (QueryHandlerDelegate<TResponse>)HandleTask,
                (next, pipeline) => async () =>
                    await pipeline
                        .Handle((TQuery)query, next, cancellationToken)
                        .ConfigureAwait(false)
            )()
            .ConfigureAwait(false);
    }
}