using EventStore.Client;
using EventStore.Library.Core.Event;

namespace EventStore.Library.Core.Domain.Aggregate;

public abstract class AggregateRoot<TId, TBaseEvent, TState> : IAggregateRoot<TId, TBaseEvent>
    where TBaseEvent : IDomainEvent<TId>
    where TId : StreamId
    where TState : AggregateRootState<TId>, new()
{
    public TId Id => State.Id ?? throw new InvalidOperationException($"Id not set for aggregate root: {GetType().Name}");
    public string? LastUpdatedBy => State.LastUpdatedBy;
    public DateTime? LastUpdated => State.LastUpdated;
    protected TState State { get; set; } = new();
    protected IList<TBaseEvent> AllEvents { get; private set; } = new List<TBaseEvent>();
    protected IList<TBaseEvent> UncommittedEvents { get; } = new List<TBaseEvent>();
    public StreamPosition StreamPosition { get; private set; } = StreamPosition.Start;
    public bool StreamExists { get; private set; } = false;
    private bool _initialized;

    public static implicit operator TState(AggregateRoot<TId, TBaseEvent, TState> aggregate) => aggregate.State;

    private readonly AggregateRootStateModifier<TBaseEvent, TId, TState> _stateModifier = new();

    protected abstract void RegisterStateModification();

    protected void When<TEvent>(Func<TState, TEvent, TState> modifyState) where TEvent : TBaseEvent
    {
        _stateModifier.When(modifyState);
    }

    public IAggregateRoot<TId, TBaseEvent> StreamCreated()
    {
        StreamExists = true;
        return this;
    }

    public IAggregateRoot<TId, TBaseEvent> LoadFromEvents(
        IEnumerable<TBaseEvent> events,
        StreamPosition streamPosition)
    {
        var eventsToApply = events.ToArray();
        StreamPosition = streamPosition;
        StreamExists = true;
        if (!_initialized)
            Initialize();

        State = _stateModifier.ApplyEvents(eventsToApply, State);
        AllEvents = AllEvents
            .Concat(eventsToApply)
            .ToList();

        return this;
    }

    private void Initialize()
    {
        SetUp();
        _initialized = true;
    }

    public IAggregateRoot<TId, TBaseEvent> Create(TBaseEvent @event)
    {
        if (!_initialized)
            Initialize();

        if (AllEvents.Any())
            throw new AggregateRootAlreadyCreatedException(GetType().Name);

        AddEvent(@event);
        if (State.Id is null)
            throw new AggregateRootIdNotSetException(GetType().Name);

        return this;
    }

    public IAggregateRoot<TId, TBaseEvent> AddEvent(TBaseEvent @event)
    {
        State = _stateModifier.ApplyEvent(State, @event);
        UncommittedEvents.Add(@event);
        AllEvents.Add(@event);
        return this;
    }

    public IList<EventData> GetUncommittedDataEvents(bool shouldProcess = true) =>
        UncommittedEvents
            .Select(@event => {
                @event.ToMetaData().ShouldProcess = shouldProcess;
                return @event;
            })
            .Select(@event => @event.ToDataModel())
            .ToList();

    public TBaseEvent[] GetUncommittedEvents() => UncommittedEvents.ToArray();

    public bool HasUncommittedEvents() => UncommittedEvents.Any();

    public TBaseEvent[] GetAllEvents() => AllEvents.ToArray();

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