using EventStore.Client;
using EventStore.Library.Core.Event;

namespace EventStore.Library.Core.Domain.Aggregate;

public abstract class AggregateRoot<TId, TBaseEvent, TState> : IAggregateRoot<TId, TBaseEvent>
    where TBaseEvent : DomainEvent<TId>
    where TId : StreamId
    where TState : AggregateRootState, new()
{
    public TId Id { get; protected set; }
    public string? LastUpdatedBy => State.LastUpdatedBy;
    public DateTime LastUpdated => State.LastUpdated;
    protected TState State { get; set; } = new();
    protected readonly IList<TBaseEvent> AllEvents = new List<TBaseEvent>();
    protected readonly IList<TBaseEvent> UncommittedEvents = new List<TBaseEvent>();
    public StreamPosition StreamPosition { get; private set; } = StreamPosition.Start;
    private bool _initialized;

    private readonly AggregateRootStateModifier<TBaseEvent, TId, TState> _stateModifier = new();

    protected abstract void RegisterStateModification();

    protected void When<TEvent>(Action<TEvent> modifyStateAction) where TEvent : TBaseEvent
    {
        _stateModifier.When(modifyStateAction);
    }

    public void LoadFromEvents(
        TId id,
        IEnumerable<TBaseEvent> events,
        StreamPosition streamPosition)
    {
        Id = id;
        StreamPosition = streamPosition;
        if (!_initialized)
            Initialize();

        _stateModifier.ApplyEvents(events, State, (@event) => AllEvents.Add(@event));
    }

    private void Initialize()
    {
        SetUp();
        _initialized = true;
    }

    public void Create(TId id, TBaseEvent @event)
    {
        Id = id;
        if (!_initialized)
            Initialize();

        AddEvent(@event);
    }

    public void AddEvent(TBaseEvent @event)
    {
        _stateModifier.ApplyEvent(@event, State);
        UncommittedEvents.Add(@event);
        AllEvents.Add(@event);
    }

    public IList<EventData> GetUncommittedDataEvents(bool shouldProcess = true) =>
        UncommittedEvents
            .Select(@event => {
                @event.MetaData.ShouldProcess = shouldProcess;
                return @event;
            })
            .Select(@event => @event.ToDataModel())
            .ToList();

    public IList<TBaseEvent> GetUncommittedEvents() => UncommittedEvents;

    public bool HasUncommittedEvents() => UncommittedEvents.Any();

    public IList<TBaseEvent> GetAllEvents() => AllEvents;

    public void EventsCommitted()
    {
        StreamPosition += (ulong)UncommittedEvents.Count;
        UncommittedEvents.Clear();
    }

    public void ClearUncommittedEvents() =>
        UncommittedEvents.Clear();

    private void ValidateEvents() =>
        _stateModifier.ValidateEvents();

    private void SetUp()
    {
        RegisterStateModification();
        ValidateEvents();
    }
}