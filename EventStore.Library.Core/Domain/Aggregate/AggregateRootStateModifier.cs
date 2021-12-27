using System.Collections.Immutable;
using System.Reflection;
using EventStore.Library.Core.Event;

namespace EventStore.Library.Core.Domain.Aggregate;

public class AggregateRootStateModifier<TBaseEvent, TId, TState>
    where TBaseEvent : IDomainEvent<TId>
    where TId : StreamId
    where TState : AggregateRootState, new()
{
    public static readonly IReadOnlyCollection<Type> EventTypes = AppDomain
        .CurrentDomain
        .GetAssemblies()
        .SelectMany(assembly => assembly.GetTypes())
        .Where(type => !type.IsAbstract &&
                       !type.IsInterface &&
                       type.IsAssignableFrom(typeof(TBaseEvent))
        )
        .ToImmutableArray();

    private readonly IDictionary<string, IAggregateRootEventHandler<TId>> _modifyStateHandlers = new Dictionary<string, IAggregateRootEventHandler<TId>>();

    public void When<TEvent>(Action<TEvent> modifyStateAction) where TEvent : TBaseEvent
    {
        if (modifyStateAction == null)
            throw new ArgumentNullException(nameof(modifyStateAction));

        var eventName = typeof(TEvent).Name;

        if (_modifyStateHandlers.ContainsKey(eventName))
            throw new ArgumentException($"There is already an modify state action for event {eventName}");

        _modifyStateHandlers.Add(
            typeof(TEvent).Name,
            new AggregateRootEventHandler<TEvent, TId>((@event) => modifyStateAction(@event))
        );
    }

    public void ApplyEvents(IEnumerable<TBaseEvent> events, TState state, Action<TBaseEvent> eventProcessed)
    {
        foreach (var @event in events)
        {
            ApplyEvent(@event, state);
            eventProcessed(@event);
        }
    }

    public void ApplyEvent(TBaseEvent @event, TState state)
    {
        var eventType = @event.GetType();
        var shouldNotModifyState = eventType
            .GetCustomAttribute<IgnoreStateModificationAttribute>() is not null;

        if (shouldNotModifyState)
            return;

        var handler = _modifyStateHandlers.ContainsKey(eventType.Name)
            ? _modifyStateHandlers[eventType.Name]
            : throw new DomainEventNotHandledException(eventType.Name);

        UpdateLastUpdated(@event, state);
        handler.ModifyState(@event);
    }

    public void ValidateEvents()
    {
        var notHandledEvent = EventTypes
            .Where(eventType => eventType.GetCustomAttribute<IgnoreStateModificationAttribute>() is null)
            .FirstOrDefault(eventType => !_modifyStateHandlers.ContainsKey(eventType.Name));

        if (notHandledEvent is not null)
            throw new DomainEventNotHandledException(notHandledEvent.Name);
    }

    private static void UpdateLastUpdated(TBaseEvent @event, TState state)
    {
        if (@event.By is null)
            throw new InvalidEventByMissingException(@event.Id, @event.EventType);

        state.LastUpdatedBy = @event.By;
        state.LastUpdated = @event.Date;
    }
}