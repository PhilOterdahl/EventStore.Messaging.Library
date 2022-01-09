using EventStore.Library.Core.Event;

namespace EventStore.Library.Tests.Core.TestAggregates.Events;

public interface IUserEvent : IDomainEvent<UserId>
{
}

public abstract class UserEvent : DomainEvent<UserId>, IUserEvent
{
    protected UserEvent(UserId streamId, string by) : base(streamId, by)
    {
    }
}