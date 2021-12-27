namespace EventStore.Library.Core.Event;

public interface IDomainEvent<out TId> : IEventStoreEvent where TId : StreamId
{
    public TId StreamId { get; }
}