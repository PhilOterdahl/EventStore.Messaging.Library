using System.Runtime.CompilerServices;
using EventStore.Client;
using EventStore.Library.Core.Domain.Aggregate;
using EventStore.Library.Core.Event;

namespace EventStore.Library.Core;

public interface IEventStore
{
    public Task<TAggregate> Load<TId, TAggregate, TEvent>(TId id,
        StreamPosition position = default,
        int maxEvents = 100,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent<TId>
        where TId : StreamId
        where TAggregate : class, IAggregateRoot<TId, TEvent>, new();

    public IAsyncEnumerable<TAggregate> Load<TId, TAggregate, TEvent>(
        IEnumerable<TId> ids,
        StreamPosition position = default,
        int maxEvents = 100,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent<TId>
        where TId : StreamId
        where TAggregate : class, IAggregateRoot<TId, TEvent>, new();

    public Task<TAggregate?> TryLoad<TId, TAggregate, TEvent>(
        TId id,
        StreamPosition position = default,
        long maxEvents = 100,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent<TId>
        where TId : StreamId
        where TAggregate : class, IAggregateRoot<TId, TEvent>, new();

    public Task<TAggregate[]> LoadInParallel<TId, TAggregate, TEvent>(
        IEnumerable<TId> ids,
        StreamPosition position = default,
        int maxEvents = 100,
        int? maxDegreeOfParallelism = null,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent<TId>
        where TId : StreamId
        where TAggregate : class, IAggregateRoot<TId, TEvent>, new();

    public Task CommitEvents<TId, TAggregate, TEvent>(
        TAggregate aggregate,
        StreamState streamState,
        int batchSize = 2000,
        bool shouldProcess = true,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent<TId>
        where TId : StreamId
        where TAggregate : class, IAggregateRoot<TId, TEvent>, new();

    public Task CommitEvents<TId, TAggregate, TEvent>(
        TAggregate aggregate,
        StreamRevision revision,
        int batchSize = 2000,
        bool shouldProcess = true,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent<TId>
        where TId : StreamId
        where TAggregate : class, IAggregateRoot<TId, TEvent>, new();

    public Task CommitEvents<TId, TAggregate, TEvent>(
        IEnumerable<TAggregate> aggregates,
        StreamState streamState,
        int batchSize = 2000,
        bool shouldProcess = true,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent<TId>
        where TId : StreamId
        where TAggregate : class, IAggregateRoot<TId, TEvent>, new();

    public Task CommitEventsParallel<TId, TAggregate, TEvent>(
        IEnumerable<TAggregate> aggregates,
        StreamState streamState,
        int? maxDegreeOfParallelism = null,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent<TId>
        where TId : StreamId
        where TAggregate : class, IAggregateRoot<TId, TEvent>, new();

    public Task CommitEventsInOrder<TId, TAggregate, TEvent>(
        TAggregate aggregate,
        int batchSize = 2000,
        bool shouldProcess = true,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent<TId>
        where TId : StreamId
        where TAggregate : class, IAggregateRoot<TId, TEvent>, new();

    public Task CommitEventsInOrderParallel<TId, TAggregate, TEvent>(
        IEnumerable<TAggregate> aggregates,
        int? maxDegreeOfParallelism = null,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent<TId>
        where TId : StreamId
        where TAggregate : class, IAggregateRoot<TId, TEvent>, new();
}