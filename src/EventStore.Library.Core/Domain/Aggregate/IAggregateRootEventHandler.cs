using EventStore.Library.Core.Event;

namespace EventStore.Library.Core.Domain.Aggregate;

public interface IAggregateRootEventHandler<in TId, TAggregateRootState>
    where TId : StreamId
    where TAggregateRootState : AggregateRootState<TId>
{
    public TAggregateRootState ModifyState(TAggregateRootState state, IDomainEvent<TId> @event);
}