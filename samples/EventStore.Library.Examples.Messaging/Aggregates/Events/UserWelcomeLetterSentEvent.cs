namespace EventStore.Library.Examples.Messaging.Aggregates.Events;

public class UserWelcomeLetterSentEvent : UserEvent
{
    public UserWelcomeLetterSentEvent(UserId streamId, string @by) : base(streamId, @by)
    {
    }
}