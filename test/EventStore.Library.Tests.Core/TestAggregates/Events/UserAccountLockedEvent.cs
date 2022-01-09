using EventStore.Library.Core.Domain.Aggregate;

namespace EventStore.Library.Tests.Core.TestAggregates.Events;

[EventStoreEvent(Version = 2)]
internal class UserAccountLockedEvent : UserEvent
{
    public UserAccountLockedEvent(UserId streamId, string @by) : base(streamId, @by)
    {
    }
}