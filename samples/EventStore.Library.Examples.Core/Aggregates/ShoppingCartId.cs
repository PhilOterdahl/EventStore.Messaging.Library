using EventStore.Library.Core;

namespace EventStore.Library.Examples.Core.Aggregates;

public class ShoppingCartId : StreamId
{
    public ShoppingCartId(string value) : base(value)
    {
    }

    public static ShoppingCartId Create() => new(Guid.NewGuid().ToString());

    public override string Category => "User";
}