using Microsoft.Extensions.DependencyInjection;

namespace EventStore.Library.Messaging;

internal class MessagePipelineHandler<TMessage> where TMessage : IMessage
{
    private readonly IMessagePipelineBehavior<TMessage>[] _behaviors;

    public MessagePipelineHandler(IServiceProvider serviceProvider)
    {
        _behaviors = serviceProvider
            .GetServices<IMessagePipelineBehavior<TMessage>>()
            .ToArray();
    }

    public async Task<TMessage> Handle(
        TMessage message,
        CancellationToken cancellationToken = default)
    {
        Task<TMessage> PipelineHandleTask() => Task.FromResult(message);

        return await _behaviors
            .Reverse()
            .Aggregate(
                (MessageHandlerDelegate<TMessage>)PipelineHandleTask,
                (next, pipeline) => async () => await pipeline
                    .Handle(message, next, cancellationToken)
                    .ConfigureAwait(false)
            )()
            .ConfigureAwait(false);
    }
}