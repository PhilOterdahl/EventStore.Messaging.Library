namespace EventStore.Library.Tests.Core.TestAggregates.Events;

internal class UserContactInformationEditedEvent : UserEvent
{
    public string Email { get; }

    public UserContactInformationEditedEvent(
        UserId streamId,
        string email,
        string @by) : base(streamId, @by)
    {
        Email = email;
    }
}