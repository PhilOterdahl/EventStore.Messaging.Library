namespace EventStore.Library.Examples.Messaging.Aggregates.Events;

public class UserEnrolledEvent : UserAsyncEvent
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

    public void Deconstruct(
        out UserId userId,
        out string firstName,
        out string lastName,
        out string email,
        out string by)
    {
        userId = StreamId;
        firstName = FirstName;
        lastName = LastName;
        email = Email;
        by = By;
    }
}