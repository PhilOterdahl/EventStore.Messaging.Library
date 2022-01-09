using EventStore.Library.Core.Event;

namespace EventStore.Library.Tests.Core.TestAggregates.Events;

public interface IUserEvent : IDomainEvent<UserId>
{
}

public class UserEvent : DomainEvent<UserId>, IUserEvent
{
    public UserEvent(UserId streamId, string by) : base(streamId, by)
    {
    }
}