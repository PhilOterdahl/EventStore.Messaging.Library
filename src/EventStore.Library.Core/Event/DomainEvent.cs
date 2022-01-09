namespace EventStore.Library.Core.Event;

public abstract class DomainEvent<TId> : EventStoreEvent, IDomainEvent<TId>
    where TId : StreamId
{
    public virtual TId StreamId { get; }

    protected DomainEvent(TId streamId, string @by) : base(@by)
    {
        StreamId = streamId;
    }
}