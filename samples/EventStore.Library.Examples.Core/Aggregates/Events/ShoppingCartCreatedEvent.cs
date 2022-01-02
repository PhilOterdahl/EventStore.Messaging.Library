namespace EventStore.Library.Examples.Core.Aggregates.Events;

internal class ShoppingCartCreatedEvent : ShoppingCartEvent
{
    public ShoppingCartCreatedEvent(ShoppingCartId streamId, string @by) : base(streamId, @by)
    {
    }
}