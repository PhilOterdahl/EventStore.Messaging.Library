using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using EventStore.Client;
using EventStore.Library.Core.Domain.Aggregate;
using EventStore.Library.Core.Event;

namespace EventStore.Library.Core;

public class EventStore : IEventStore
{
    private readonly EventStoreClient _eventStoreClient;

    public EventStore(EventStoreClient eventStoreClient)
    {
        _eventStoreClient = eventStoreClient;
    }

    public async Task<TAggregate> Load<TId, TAggregate, TEvent>(TId id,
        StreamPosition position = default,
        int maxEvents = 100,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent<TId>
        where TId : StreamId
        where TAggregate : class, IAggregateRoot<TId, TEvent>, new()
    {
        var aggregate = await TryLoad<TId, TAggregate, TEvent>(id, position, maxEvents, cancellationToken);

        if (aggregate is null)
            throw new StreamNotFoundException(id);

        return aggregate;
    }

    public async IAsyncEnumerable<TAggregate> Load<TId, TAggregate, TEvent>(
        IEnumerable<TId> ids,
        StreamPosition position = default,
        int maxEvents = 100,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent<TId>
        where TId : StreamId
        where TAggregate : class, IAggregateRoot<TId, TEvent>, new()
    {
        foreach (var id in ids.ToArray())
            yield return await Load<TId, TAggregate, TEvent>(id, position, maxEvents, cancellationToken);
    }

    public async Task<TAggregate?> TryLoad<TId, TAggregate, TEvent>(
        TId id,
        StreamPosition position = default,
        long maxEvents = 100,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent<TId>
        where TId : StreamId
        where TAggregate : class, IAggregateRoot<TId, TEvent>, new()
    {
        if (id is null)
            throw new ArgumentNullException(nameof(id));

        await using var response = _eventStoreClient.ReadStreamAsync(
            Direction.Forwards,
            id.ToString(),
            position,
            maxEvents,
            cancellationToken: cancellationToken);

        var status = await response.ReadState;

        if (status == ReadState.StreamNotFound)
            return null;

        var resolvedEvents = await response.ToArrayAsync(cancellationToken);
        var events = resolvedEvents.SelectEvents<TEvent>();

        var aggregate = new TAggregate();
        aggregate.LoadFromEvents(id, events, resolvedEvents.Last().Event.EventNumber);

        return aggregate;
    }

    public async Task<TAggregate[]> LoadInParallel<TId, TAggregate, TEvent>(
        IEnumerable<TId> ids,
        StreamPosition position = default,
        int maxEvents = 100,
        int? maxDegreeOfParallelism = null,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent<TId>
        where TId : StreamId
        where TAggregate : class, IAggregateRoot<TId, TEvent>, new()
    {
        var aggregates = new ConcurrentBag<TAggregate>();
        var parallelOptions = GetParallelOptions(cancellationToken, maxDegreeOfParallelism);

        await Parallel.ForEachAsync(ids, parallelOptions, async (id, token) =>
        {
            var aggregate = await Load<TId, TAggregate, TEvent>(id, position, maxEvents, token);
            aggregates.Add(aggregate);
        });

        return aggregates.ToArray();
    }

    public async Task CommitEvents<TId, TAggregate, TEvent>(
        TAggregate aggregate,
        StreamState streamState,
        int batchSize = 2000,
        bool shouldProcess = true,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent<TId>
        where TId : StreamId
        where TAggregate : class, IAggregateRoot<TId, TEvent>, new()
    {
        var events = aggregate.GetUncommittedDataEvents(shouldProcess);

        if (events.Count < batchSize)
            await _eventStoreClient.AppendToStreamAsync(
                aggregate.Id.ToString(),
                streamState,
                events,
                cancellationToken: cancellationToken);
        else
            foreach (var batch in events.Chunk(batchSize))
                await _eventStoreClient.AppendToStreamAsync(
                    aggregate.Id.ToString(),
                    streamState,
                    batch,
                    cancellationToken: cancellationToken);

        aggregate.EventsCommitted();
    }

    public async Task CommitEvents<TId, TAggregate, TEvent>(
        TAggregate aggregate,
        StreamRevision revision,
        int batchSize = 2000,
        bool shouldProcess = true,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent<TId>
        where TId : StreamId
        where TAggregate : class, IAggregateRoot<TId, TEvent>, new()
    {
        var events = aggregate.GetUncommittedDataEvents(shouldProcess);

        if (events.Count < batchSize)
        {
            await _eventStoreClient.AppendToStreamAsync(
                aggregate.Id.ToString(),
                revision,
                events,
                cancellationToken: cancellationToken);
        }
        else
        {
            var currentRevision = revision;
            foreach (var batch in events.Chunk(batchSize))
            {
                await _eventStoreClient.AppendToStreamAsync(
                    aggregate.Id.ToString(),
                    currentRevision,
                    batch,
                    cancellationToken: cancellationToken);
                currentRevision += (ulong)batch.Length;
            }
        }

        aggregate.EventsCommitted();
    }

    public async Task CommitEvents<TId, TAggregate, TEvent>(
        IEnumerable<TAggregate> aggregates,
        StreamState streamState,
        int batchSize = 2000,
        bool shouldProcess = true,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent<TId>
        where TId : StreamId
        where TAggregate : class, IAggregateRoot<TId, TEvent>, new()
    {
        foreach (var aggregate in aggregates.ToArray())
            await CommitEvents<TId, TAggregate, TEvent>(aggregate, streamState, batchSize, shouldProcess,
                cancellationToken);
    }

    public async Task CommitEventsParallel<TId, TAggregate, TEvent>(
        IEnumerable<TAggregate> aggregates,
        StreamState streamState,
        int? maxDegreeOfParallelism = null,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent<TId>
        where TId : StreamId
        where TAggregate : class, IAggregateRoot<TId, TEvent>, new()
    {
        var parallelOptions = GetParallelOptions(cancellationToken, maxDegreeOfParallelism);

        await Parallel.ForEachAsync(aggregates, parallelOptions, async (aggregate, token) =>
            await CommitEvents<TId, TAggregate, TEvent>(aggregate, streamState, cancellationToken: token)
        );
    }

    public async Task CommitEventsInOrder<TId, TAggregate, TEvent>(
        TAggregate aggregate,
        int batchSize = 2000,
        bool shouldProcess = true,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent<TId>
        where TId : StreamId
        where TAggregate : class, IAggregateRoot<TId, TEvent>, new()
    {
        async Task AppendToStreamInOrder(IList<EventData> events)
        {
            var result = aggregate.StreamPosition == StreamPosition.Start
                ? await _eventStoreClient.AppendToStreamAsync(
                    aggregate.Id.ToString(),
                    StreamState.NoStream,
                    events,
                    cancellationToken: cancellationToken)
                : await _eventStoreClient.AppendToStreamAsync(
                    aggregate.Id.ToString(),
                    StreamRevision.FromStreamPosition(aggregate.StreamPosition),
                    events,
                    cancellationToken: cancellationToken);
        }

        var events = aggregate.GetUncommittedDataEvents(shouldProcess);

        if (events.Count < batchSize)
        {
            await AppendToStreamInOrder(events);
        }
        else
        {
            var currentRevision = StreamRevision.FromStreamPosition(aggregate.StreamPosition);
            foreach (var batch in events.Chunk(batchSize))
            {
                await AppendToStreamInOrder(events);
                currentRevision += (ulong)batch.Length;
            }
        }

        aggregate.EventsCommitted();
    }

    public async Task CommitEventsInOrderParallel<TId, TAggregate, TEvent>(
        IEnumerable<TAggregate> aggregates,
        int? maxDegreeOfParallelism = null,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent<TId>
        where TId : StreamId
        where TAggregate : class, IAggregateRoot<TId, TEvent>, new()
    {
        var parallelOptions = GetParallelOptions(cancellationToken, maxDegreeOfParallelism);
        await Parallel.ForEachAsync(aggregates, parallelOptions, async (aggregate, token) =>
            await CommitEvents<TId, TAggregate, TEvent>(aggregate,
                StreamRevision.FromStreamPosition(aggregate.StreamPosition), cancellationToken: token)
        );
    }

    private static ParallelOptions GetParallelOptions(CancellationToken cancellationToken, int? maxDegreeOfParallelism) =>
        new()
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = maxDegreeOfParallelism ?? Environment.ProcessorCount / 2
        };
}