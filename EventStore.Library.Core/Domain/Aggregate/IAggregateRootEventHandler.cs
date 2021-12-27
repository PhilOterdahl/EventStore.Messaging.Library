using EventStore.Library.Core.Event;

namespace EventStore.Library.Core.Domain.Aggregate;

public interface IAggregateRootEventHandler<in TId>
    where TId : StreamId
{
    public void ModifyState(IDomainEvent<TId> @event);
}