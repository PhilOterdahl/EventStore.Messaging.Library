using EventStore.Client;
using EventStore.Library.Core.Event;

namespace EventStore.Library.Core.Domain.Aggregate;

public interface IAggregateRoot<TId, in TEvent>
    where TId : StreamId
    where TEvent : IDomainEvent<TId>
{
    public TId Id { get; }
    public IList<EventData> GetUncommittedDataEvents(bool shouldProcess = true);
    public StreamPosition StreamPosition { get; }
    public void EventsCommitted();
    public void LoadFromEvents(TId id, IEnumerable<TEvent> events, StreamPosition streamPosition);
}