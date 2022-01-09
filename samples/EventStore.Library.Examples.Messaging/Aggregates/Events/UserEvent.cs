using EventStore.Library.Core.Event;
using EventStore.Library.Messaging.Event;

namespace EventStore.Library.Examples.Messaging.Aggregates.Events;

public interface IUserEvent : IDomainEvent<UserId>
{
}

public abstract class UserEvent : DomainEvent<UserId>, IUserEvent
{
    protected UserEvent(UserId streamId, string by) : base(streamId, by)
    {
    }
}

public abstract class UserAsyncEvent : AsyncDomainEvent<UserId>, IUserEvent
{
    protected UserAsyncEvent(UserId streamId, string by) : base(streamId, by)
    {
    }
}