using EventStore.Library.Core.Domain.Aggregate;
using EventStore.Library.Examples.Core.Aggregates.Events;

namespace EventStore.Library.Examples.Core.Aggregates;

internal class ShoppingCart : AggregateRoot<ShoppingCartId, IShoppingCartEvent, ShoppingCartState>
{
    public ShoppingCart()
    {
    }

    public static ShoppingCart Create(string by)
    {
        var shoppingCart = new ShoppingCart();
        return shoppingCart.CreateShoppingCart(by);
    }

    private ShoppingCart CreateShoppingCart(string by)
    {
        var cartCreated = new ShoppingCartCreatedEvent(ShoppingCartId.Create(), by);
        return (ShoppingCart)Create(cartCreated);
    }

    public ShoppingCart AddItem(string item, string by)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        if (string.IsNullOrWhiteSpace(item))
            throw new ArgumentException("Item can not be empty or whitespace", nameof(item));

        var itemAdded = new ItemAddedToShoppingCartEvent(Id, item, by);
        return (ShoppingCart)AddEvent(itemAdded);
    }

    public ShoppingCart RemoveItem(string item, string by)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        if (string.IsNullOrWhiteSpace(item))
            throw new ArgumentException("Item can not be empty or whitespace", nameof(item));

        var itemExists = State.Items.Any(i => i == item);

        if (!itemExists)
            throw new InvalidOperationException("Shopping cart item does not exists in shopping cart");

        var itemRemoved = new ItemRemovedFromShoppingCartEvent(Id, item, by);
        return (ShoppingCart)AddEvent(itemRemoved);
    }

    protected override void RegisterStateModification()
    {
        When<ShoppingCartCreatedEvent>((state, @event) =>
        {
            state.Id = @event.StreamId;
            return state;
        });

        When<ItemAddedToShoppingCartEvent>((state, @event) =>
        {
            state.Items.Add(@event.Item);
            return state;
        });

        When<ItemRemovedFromShoppingCartEvent>((state, @event) =>
        {
            state.Items = state
                .Items
                .Where(item => item !=@event.Item)
                .ToList();

            return state;
        });
    }
}