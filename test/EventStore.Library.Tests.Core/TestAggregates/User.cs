using System;
using EventStore.Library.Core.Domain.Aggregate;
using EventStore.Library.Tests.Core.TestAggregates.Events;

namespace EventStore.Library.Tests.Core.TestAggregates;

public class User : AggregateRoot<UserId, IUserEvent, UserState>
{
    public User()
    {
    }

    public User Enroll(
        string firstName,
        string lastName,
        string email,
        string by)
    {
        var userEnrolled = new UserEnrolledEvent(UserId.Create(), firstName, lastName, email, by);
        return (User)Create(userEnrolled);
    }

    public User EditContactInformation(string email, string by)
    {
        if (email is null)
            throw new ArgumentNullException(nameof(email));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email can not be empty or whitespace", nameof(email));

        var contactInformationEdited = new UserContactInformationEditedEvent(Id, email, by);
        return (User)AddEvent(contactInformationEdited);
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

        When<UserContactInformationEditedEvent>((state, @event) =>
        {
            state.Email = @event.Email;
            return state;
        });
    }
}