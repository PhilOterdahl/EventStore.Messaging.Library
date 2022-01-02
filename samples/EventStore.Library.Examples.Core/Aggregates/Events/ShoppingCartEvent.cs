using EventStore.Library.Core.Event;

namespace EventStore.Library.Examples.Core.Aggregates.Events;

public interface IShoppingCartEvent : IDomainEvent<ShoppingCartId>
{
}

public class ShoppingCartEvent : DomainEvent<ShoppingCartId>, IShoppingCartEvent
{
    public ShoppingCartEvent(ShoppingCartId streamId, string by) : base(streamId, by)
    {
    }
}