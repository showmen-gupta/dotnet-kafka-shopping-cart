using ApacheKafkaBasics.Interfaces;
using ApacheKafkaBasics.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApacheKafkaBasics.Controllers;

[ApiController]
[Route("[controller]")]
public class CartController(IShoppingCart shoppingCart) : Controller
{
    [HttpPost("AddToCart")]
    public async Task<IActionResult> AddToCart(Product product, int quantity)
    {
        await shoppingCart.AddProduct(product, quantity);
        return Ok("Successfully Added product: " + product.Name);
    }

    [HttpDelete("RemoveFromCart")]
    public async Task<IActionResult> RemoveFromCart(int productId)
    {
        await shoppingCart.RemoveProduct(productId);
        return Ok("Successfully Added removed product with productId: " + productId);
    }

    [HttpGet("GetTotalPrice")]
    public async Task<IActionResult> GetTotalPrice()
    {
        var totalPrice = await shoppingCart.GetTotalPrice();
        return Ok(totalPrice);
    }

    [HttpGet("GetCartItems")]
    public async Task<IActionResult> GetCartItems()
    {
        var cartItems = await shoppingCart.GetCartItems();
        if (cartItems.Count > 0)
            return Ok(cartItems);
        return NotFound("No item found in the cart");
    }
}