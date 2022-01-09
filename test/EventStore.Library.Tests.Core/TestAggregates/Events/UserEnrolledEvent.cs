namespace EventStore.Library.Tests.Core.TestAggregates.Events;

public class UserEnrolledEvent : UserEvent
{
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }

    public UserEnrolledEvent(
        UserId streamId,
        string firstName,
        string lastName,
        string email,
        string @by) : base(streamId, @by)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }
}