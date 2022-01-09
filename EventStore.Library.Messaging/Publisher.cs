using EventStore.Client;
using EventStore.Library.Core;
using EventStore.Library.Messaging.Command;
using EventStore.Library.Messaging.Event;
using EventStore.Library.Messaging.Threading;

namespace EventStore.Library.Messaging
{
    internal class Publisher : IPublisher
    {
        private readonly IEventStore _eventStore;
        private readonly IServiceProvider _serviceProvider;

        public Publisher(IEventStore eventStore, IServiceProvider serviceProvider)
        {
            _eventStore = eventStore;
            _serviceProvider = serviceProvider;
        }

        public async Task Enqueue(IAsyncCommand asyncCommand, CancellationToken cancellationToken = default)
        {
            var pipelineHandler = (IAsyncCommandPipelineHandler?)Activator.CreateInstance(
                typeof(AsyncCommandPipelineHandler<>).MakeGenericType(asyncCommand.GetType()),
                _serviceProvider
            );

            if (pipelineHandler is null)
                throw new InvalidOperationException("AsyncCommandPipelineHandler can not be null");

            asyncCommand = await pipelineHandler.PreProcess(asyncCommand, cancellationToken);

            await _eventStore.AppendEventsToStream(
                new AsyncCommandStreamId(),
                StreamState.Any,
                cancellationToken,
                asyncCommand.ToDataModel()
            );
        }

        public async Task Enqueue<TAsyncCommand>(
            TAsyncCommand[] asyncCommands,
            int maxDegreeOfParallelism = 2,
            int batchSize = 1000,
            CancellationToken cancellationToken = default) where TAsyncCommand : IAsyncCommand
        {
            var messages = await asyncCommands
                .Select(async asyncCommand =>
                {
                    var pipelineHandler = (IAsyncCommandPipelineHandler?)Activator.CreateInstance(
                        typeof(AsyncCommandPipelineHandler<>).MakeGenericType(asyncCommand.GetType()),
                        _serviceProvider
                    );

                    if (pipelineHandler is null)
                        throw new InvalidOperationException("AsyncCommandPipelineHandler can not be null");

                    var command = await pipelineHandler.PreProcess(asyncCommand, cancellationToken);
                    return command.ToDataModel();
                })
                .ToArrayAsyncParallel(cancellationToken: cancellationToken);

            if (messages.Length > batchSize)
            {
                var parallelOptions = new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = maxDegreeOfParallelism
                };
                var batches = messages.Chunk(batchSize);

                await Parallel.ForEachAsync(batches, parallelOptions, async (batch, token) =>
                    await _eventStore.AppendEventsToStream(
                        new AsyncCommandStreamId(),
                        StreamState.Any,
                        token,
                        batch
                    ));
            }
            else
            {
                await _eventStore.AppendEventsToStream(
                    new AsyncCommandStreamId(),
                    StreamState.Any,
                    cancellationToken,
                    messages
                );
            }
        }

        public async Task Publish(IAsyncEvent asyncEvent, CancellationToken cancellationToken = default)
        {
            var pipelineHandler = (IAsyncEventPipelineHandler?)Activator.CreateInstance(
                typeof(AsyncEventPipelineHandler<>).MakeGenericType(asyncEvent.GetType()),
                _serviceProvider
            );

            if (pipelineHandler is null)
                throw new InvalidOperationException("AsyncEventPipelineHandler can not be null");

            asyncEvent = await pipelineHandler.PreProcess(asyncEvent, cancellationToken);

            await _eventStore.AppendEventsToStream(
                new AsyncEventStreamId(),
                StreamState.Any,
                cancellationToken,
                asyncEvent.ToDataModel()
            );
        }
    }
}
