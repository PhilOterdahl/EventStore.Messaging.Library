using EventStore.Library.Core.Domain.Aggregate;

namespace EventStore.Library.Examples.Core.Aggregates;

public class ShoppingCartState : AggregateRootState<ShoppingCartId>
{
    public IList<string> Items { get; set; } = new List<string>();
}