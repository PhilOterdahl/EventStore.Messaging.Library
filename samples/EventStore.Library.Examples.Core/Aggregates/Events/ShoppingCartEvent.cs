using EventStore.Library.Core.Event;

namespace EventStore.Library.Examples.Core.Aggregates.Events;

public interface IShoppingCartEvent : IDomainEvent<ShoppingCartId>
{
}

public abstract class ShoppingCartEvent : DomainEvent<ShoppingCartId>, IShoppingCartEvent
{
    protected ShoppingCartEvent(ShoppingCartId streamId, string by) : base(streamId, by)
    {
    }
}