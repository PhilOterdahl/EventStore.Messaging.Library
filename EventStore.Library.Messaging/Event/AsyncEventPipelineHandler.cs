using Microsoft.Extensions.DependencyInjection;

namespace EventStore.Library.Messaging.Event;

internal class AsyncEventPipelineHandler<TAsyncEvent> : IAsyncEventPipelineHandler where TAsyncEvent : IAsyncEvent
{
    private readonly IServiceProvider _serviceProvider;

    public AsyncEventPipelineHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Handle(IAsyncEvent @event, CancellationToken cancellationToken = default)
    {
        var handlers = _serviceProvider.GetServices<IAsyncEventHandler<TAsyncEvent>>();

        if (!handlers.Any())
            throw new NoEventHandlerRegisteredException(@event.GetType().Name);

        foreach (var handler in handlers)
        {
            await handler
                .Handle((TAsyncEvent)@event, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public async Task<IAsyncEvent> PreProcess(IAsyncEvent @event, CancellationToken cancellationToken = default)
    {
        var messagePipelineHandler = (MessagePipelineHandler<TAsyncEvent>?)Activator.CreateInstance(
            typeof(MessagePipelineHandler<>).MakeGenericType(@event.GetType()),
            _serviceProvider
        );

        if (messagePipelineHandler is null)
            throw new InvalidOperationException("MessagePipelineHandler can not be null");

        return await messagePipelineHandler
            .Handle((TAsyncEvent)@event, cancellationToken)
            .ConfigureAwait(false);
    }
}