using EventStore.Library.Core;
using EventStore.Library.Examples.Core.Aggregates;
using EventStore.Library.Examples.Core.Aggregates.Events;

namespace EventStore.Library.Examples.Core.Extensions;

internal static class ShoppingCartDataExtensions
{
    public static async Task CommitEvents(
        this IEventStore eventStore, 
        ShoppingCart cart,
        CancellationToken cancellationToken = default) =>
        await eventStore.CommitEvents<ShoppingCartId, ShoppingCart, IShoppingCartEvent>(cart, cancellationToken: cancellationToken);

    public static async Task<ShoppingCart?> TryLoad(
        this IEventStore eventStore,
        ShoppingCartId id,
        CancellationToken cancellationToken = default) =>
        await eventStore.TryLoad<ShoppingCartId, ShoppingCart, IShoppingCartEvent>(id, cancellationToken: cancellationToken);
}