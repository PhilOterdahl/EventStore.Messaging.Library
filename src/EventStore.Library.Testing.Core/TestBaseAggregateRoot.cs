using EventStore.Client;
using EventStore.Library.Core;
using EventStore.Library.Core.Domain.Aggregate;
using EventStore.Library.Core.Event;
using Xunit;

namespace EventStore.Library.Testing.Core;

public class TestBaseAggregateRoot<TAggregate, TId, TBaseEvent, TState>
    where TBaseEvent : IDomainEvent<TId>
    where TId : StreamId
    where TState : AggregateRootState<TId>, new()
    where TAggregate : IAggregateRoot<TId, TBaseEvent>
{
    public TAggregate AggregateRoot { get; private set; }

    protected TestBaseAggregateRoot(TAggregate aggregateRoot)
    {
        AggregateRoot = aggregateRoot;
    }

    protected void Given(params TBaseEvent[] events)
    {
        AggregateRoot = (TAggregate)AggregateRoot.LoadFromEvents(events, StreamPosition.FromInt64(events.Length));
    }

    protected void When(Action<TAggregate> command)
    {
        command(AggregateRoot);
    }

    protected void Then<TEvent>(int index = 0, params Action<TEvent>[] conditions) where TEvent : TBaseEvent
    {
        var events = AggregateRoot.GetUncommittedEvents();

        var @event = events
            .OfType<TEvent>()
            .ElementAt(index);

        foreach (var condition in conditions)
        {
            condition(@event);
        }
    }

    protected void Then(params Action<TAggregate>[] conditions)
    {
        foreach (var condition in conditions)
        {
            condition(AggregateRoot);
        }
    }

    protected void Throws<TException>(Action<TAggregate> command, params Action<TException>[] conditions) where TException : Exception
    {
        var exception = Assert.Throws<TException>(() => command(AggregateRoot));

        foreach (var condition in conditions)
        {
            condition(exception);
        }
    }
}