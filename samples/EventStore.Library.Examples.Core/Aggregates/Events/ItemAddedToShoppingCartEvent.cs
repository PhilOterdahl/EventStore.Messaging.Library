namespace EventStore.Library.Examples.Core.Aggregates.Events;

internal class ItemAddedToShoppingCartEvent : ShoppingCartEvent
{
    public string Item { get; }

    public ItemAddedToShoppingCartEvent(ShoppingCartId streamId, string item, string @by) : base(streamId, @by)
    {
        Item = item;
    }
}