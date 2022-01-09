using EventStore.Library.Core.Event;

namespace EventStore.Library.Core.Domain.Aggregate;

public class AggregateRootEventHandler<TEvent, TId, TAggregateRootState> : IAggregateRootEventHandler<TId, TAggregateRootState>
    where TEvent : IDomainEvent<TId>
    where TId : StreamId
    where TAggregateRootState : AggregateRootState<TId>
{
    private readonly ModifyStateDelegate _modifyStateDelegate;

    public AggregateRootEventHandler(ModifyStateDelegate modifyStateDelegate)
    {
        _modifyStateDelegate = modifyStateDelegate;
    }

    public TAggregateRootState ModifyState(TAggregateRootState state, IDomainEvent<TId> @event) => _modifyStateDelegate(state, (TEvent)@event);

    public delegate TAggregateRootState ModifyStateDelegate(TAggregateRootState state, TEvent @event);
}