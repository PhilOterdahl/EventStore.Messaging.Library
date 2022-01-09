namespace EventStore.Library.Examples.Messaging.Aggregates.Events;

public class UserAccountDeletedEvent : UserEvent
{
    public UserAccountDeletedEvent(
        UserId streamId,
        string @by) : base(streamId, @by)
    {
    }
}