using EventStore.Library.Core;
using EventStore.Library.Examples.Core.Aggregates;
using EventStore.Library.Examples.Core.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace EventStore.Library.Examples.Core.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ShoppingCartController : ControllerBase
{

    private readonly IEventStore _eventStore;

    public ShoppingCartController(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ShoppingCartState>> GetShoppingCart(string id)
    {
        var shoppingCart = await _eventStore.TryLoad(new ShoppingCartId(id));

        if (shoppingCart is null)
            return NotFound();

        var state = (ShoppingCartState)shoppingCart;

        return Ok(state);
    }

    [HttpPost]
    public async Task<ActionResult<ShoppingCartState>> CreateShoppingCart(string by)
    {
        var shoppingCart = ShoppingCart.Create(by);

        await _eventStore.CommitEvents(shoppingCart);

        return Ok(shoppingCart.Id.Value);
    }

    [HttpPost("{id}/items")]
    public async Task<ActionResult<ShoppingCartState>> AddItemToShoppingCart(string id, [FromBody] AddShoppingCartItemRequest request)
    {
        var shoppingCart = await _eventStore.TryLoad(new ShoppingCartId(id));

        if (shoppingCart is null)
            return NotFound();

        shoppingCart.AddItem(request.Item, request.By);

        await _eventStore.CommitEvents(shoppingCart);

        return NoContent();
    }

    [HttpDelete("{id}/items/{itemName}")]
    public async Task<ActionResult<ShoppingCartState>> RemoveItemFromShoppingCart(string id, string itemName, [FromBody] RemoveShoppingCartItemRequest request)
    {
        var shoppingCart = await _eventStore.TryLoad(new ShoppingCartId(id));

        if (shoppingCart is null)
            return NotFound();

        shoppingCart.RemoveItem(itemName, request.By);

        await _eventStore.CommitEvents(shoppingCart);

        return NoContent();
    }
}

public class AddShoppingCartItemRequest
{
    public string? Item { get; set; }
    public string? By { get; set; }
}

public class RemoveShoppingCartItemRequest
{
    public string? By { get; set; }
}