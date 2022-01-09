using EventStore.Client;
using EventStore.Library.Core.Event;

namespace EventStore.Library.Core.Domain.Aggregate;

public interface IAggregateRoot<out TId, TEvent>
    where TId : StreamId
    where TEvent : IDomainEvent<TId>
{
    public TId Id { get; }

    public StreamPosition StreamPosition { get; }
    public bool StreamExists { get; }

    public void EventsCommitted();
    public bool HasUncommittedEvents();
    public TEvent[] GetAllEvents();
    public TEvent[] GetUncommittedEvents();
    public IList<EventData> GetUncommittedDataEvents(bool shouldProcess = true);
    public IAggregateRoot<TId, TEvent> StreamCreated();
    public IAggregateRoot<TId, TEvent> LoadFromEvents(IEnumerable<TEvent> events, StreamPosition streamPosition);
}