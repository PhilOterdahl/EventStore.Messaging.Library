using EventStore.Library.Core.Domain.Aggregate;

namespace EventStore.Library.Tests.Core.TestAggregates;

public class UserState : AggregateRootState<UserId>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
}