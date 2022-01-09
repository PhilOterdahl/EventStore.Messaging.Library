using EventStore.Library.Tests.Core.TestAggregates.Events;

namespace EventStore.Library.Tests.Core.TestAggregates.Archived;

internal class UserAccountLockedEvent : UserEvent
{
    public UserAccountLockedEvent(UserId streamId, string @by) : base(streamId, @by)
    {
    }
}