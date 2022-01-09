using EventStore.Library.Core.Domain.Aggregate;
using EventStore.Library.Examples.Messaging.Aggregates.Events;

namespace EventStore.Library.Examples.Messaging.Aggregates;

public class User : AggregateRoot<UserId, IUserEvent, UserState>
{
    public User()
    {
    }

    public static User Enroll(
        string firstName,
        string lastName,
        string email,
        string by)
    {
        var user = new User();
        return user.EnrollUser(firstName, lastName, email, by);
    }

    private User EnrollUser(
        string firstName,
        string lastName,
        string email, string by)
    {
        var userEnrolled = new UserEnrolledEvent(UserId.Create(), firstName, lastName, email, by);
        return (User)Create(userEnrolled);
    }

    public User WelcomeLetterSent(string by)
    {
        if (State.Deleted)
            throw new InvalidOperationException($"Account: {Id.Value} is deleted, welcome letter can not be sent");

        var welcomeLetterSent = new UserWelcomeLetterSentEvent(Id, by);
        return (User)AddEvent(welcomeLetterSent);
    }

    public User Delete(string by)
    {
        var deleted = new UserAccountDeletedEvent(Id, by);
        return (User)AddEvent(deleted);
    }

    protected override void RegisterStateModification()
    {
        When<UserEnrolledEvent>((state, @event) =>
        {
            state.LastName = @event.LastName;
            state.FirstName = @event.FirstName;
            state.Id = @event.StreamId;
            state.Email = @event.Email;
            return state;
        });

        When<UserWelcomeLetterSentEvent>((state, _) =>
        {
            state.WelcomeLetterSent = true;
            return state;
        });

        When<UserAccountDeletedEvent>((state, _) =>
        {
            state.Deleted = false;
            return state;
        });
    }
}