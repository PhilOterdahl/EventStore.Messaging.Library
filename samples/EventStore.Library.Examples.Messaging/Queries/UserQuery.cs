using EventStore.Library.Core;
using EventStore.Library.Examples.Messaging.Aggregates;
using EventStore.Library.Examples.Messaging.DataExtensions;
using EventStore.Library.Messaging.Query;

namespace EventStore.Library.Examples.Messaging.Queries;

internal class UserQuery : IQuery<UserState?>
{
    public UserId UserId { get; }

    public UserQuery(string userId)
    {
        UserId = new UserId(userId);
    }

    public void Deconstruct(out string userId)
    {
        userId = UserId;
    }
}

internal class UserQueryHandler : IQueryHandler<UserQuery, UserState?>
{
    private readonly IEventStore _eventStore;

    public UserQueryHandler(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<UserState?> Handle(UserQuery query, CancellationToken cancellationToken) => await _eventStore.TryLoad(query.UserId, cancellationToken);
}