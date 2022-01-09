using EventStore.Library.Core.Event;
using EventStore.Library.Messaging.Event;

namespace EventStore.Library.Examples.Messaging.Aggregates.Events;

public interface IUserEvent : IDomainEvent<UserId>
{
}

public class UserEvent : DomainEvent<UserId>, IUserEvent
{
    public UserEvent(UserId streamId, string by) : base(streamId, by)
    {
    }
}

public class UserAsyncEvent : AsyncDomainEvent<UserId>, IUserEvent
{
    public UserAsyncEvent(UserId streamId, string by) : base(streamId, by)
    {
    }
}