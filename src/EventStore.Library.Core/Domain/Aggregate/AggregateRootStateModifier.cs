using System.Reflection;
using EventStore.Library.Core.Event;

namespace EventStore.Library.Core.Domain.Aggregate;

public class AggregateRootStateModifier<TBaseEvent, TId, TState>
    where TBaseEvent : IDomainEvent<TId>
    where TId : StreamId
    where TState : AggregateRootState<TId>, new()
{
    private readonly IDictionary<string, IAggregateRootEventHandler<TId, TState>> _modifyStateHandlers = new Dictionary<string, IAggregateRootEventHandler<TId, TState>>();

    public void When<TEvent>(Func<TState, TEvent, TState> modifyStateAction) where TEvent : TBaseEvent
    {
        if (modifyStateAction == null)
            throw new ArgumentNullException(nameof(modifyStateAction));

        var eventName = typeof(TEvent).Name;

        if (_modifyStateHandlers.ContainsKey(eventName))
            throw new ArgumentException($"There is already an modify state action for event {eventName}");

        _modifyStateHandlers.Add(
            typeof(TEvent).Name,
            new AggregateRootEventHandler<TEvent, TId, TState>((state, @event) => modifyStateAction(state, @event))
        );
    }

    public TState ApplyEvents(IEnumerable<TBaseEvent> events, TState state) => events.Aggregate(state, ApplyEvent);

    public TState ApplyEvent(TState state, TBaseEvent @event)
    {
        var eventType = @event.GetType();
        var key = @event.GetEventType();
        var shouldNotModifyState = eventType
            .GetCustomAttribute<IgnoreStateModificationAttribute>() is not null;

        if (shouldNotModifyState)
            return state;

        var handler = _modifyStateHandlers.ContainsKey(key)
            ? _modifyStateHandlers[key]
            : throw new DomainEventNotHandledException(key);

        state = UpdateLastUpdated(@event, state);

        if (state.Created is null)
            state = SetCreated(@event, state);

        return handler.ModifyState(state, @event);
    }

    private static TState SetCreated(TBaseEvent @event, TState state)
    {
        state.Created = @event.Date;
        return state;
    }

    private static TState UpdateLastUpdated(TBaseEvent @event, TState state)
    {
        if (@event.By is null)
            throw new InvalidEventByMissingException(@event.Id, @event.GetEventType());

        state.LastUpdatedBy = @event.By;
        state.LastUpdated = @event.Date;

        return state;
    }
}