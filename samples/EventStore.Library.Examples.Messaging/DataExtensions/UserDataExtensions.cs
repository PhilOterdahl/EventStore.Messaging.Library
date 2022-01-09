using EventStore.Library.Core;
using EventStore.Library.Examples.Messaging.Aggregates;
using EventStore.Library.Examples.Messaging.Aggregates.Events;

namespace EventStore.Library.Examples.Messaging.DataExtensions;

internal static class UserDataExtensions
{
    public static async Task CommitEvents(
        this IEventStore eventStore,
        User user,
        CancellationToken cancellationToken = default) =>
        await eventStore.CommitEvents<UserId, User, IUserEvent>(user, cancellationToken: cancellationToken);

    public static async Task<User?> TryLoad(
        this IEventStore eventStore,
        UserId id,
        CancellationToken cancellationToken = default) =>
        await eventStore.TryLoad<UserId, User, IUserEvent>(id, cancellationToken: cancellationToken);

    public static async Task<User> Load(
        this IEventStore eventStore,
        UserId id,
        CancellationToken cancellationToken = default) =>
        await eventStore.Load<UserId, User, IUserEvent>(id, cancellationToken: cancellationToken);
}