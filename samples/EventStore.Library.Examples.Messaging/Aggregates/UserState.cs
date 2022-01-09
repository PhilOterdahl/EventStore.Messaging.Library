using EventStore.Library.Core.Domain.Aggregate;

namespace EventStore.Library.Examples.Messaging.Aggregates;

public class UserState : AggregateRootState<UserId>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public bool WelcomeLetterSent { get; set; }
    public bool Deleted { get; set; }
}