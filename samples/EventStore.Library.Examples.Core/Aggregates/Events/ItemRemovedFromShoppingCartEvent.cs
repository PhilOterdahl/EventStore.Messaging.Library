namespace EventStore.Library.Examples.Core.Aggregates.Events;

public class ItemRemovedFromShoppingCartEvent : ShoppingCartEvent
{
    public string Item { get; }

    public ItemRemovedFromShoppingCartEvent(ShoppingCartId streamId, string item, string @by) : base(streamId, @by)
    {
        Item = item;
    }
}