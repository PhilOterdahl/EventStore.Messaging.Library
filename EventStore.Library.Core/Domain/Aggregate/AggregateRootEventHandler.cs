using EventStore.Library.Core.Event;

namespace EventStore.Library.Core.Domain.Aggregate;

public class AggregateRootEventHandler<TEvent, TId> : IAggregateRootEventHandler<TId>
    where TEvent : IDomainEvent<TId>
    where TId : StreamId
{
    private readonly ModifyStateDelegate _modifyStateDelegate;

    public AggregateRootEventHandler(ModifyStateDelegate modifyStateDelegate)
    {
        _modifyStateDelegate = modifyStateDelegate;
    }

    public void ModifyState(IDomainEvent<TId> @event)
    {
        _modifyStateDelegate((TEvent)@event);
    }

    public delegate void ModifyStateDelegate(TEvent @event);
}