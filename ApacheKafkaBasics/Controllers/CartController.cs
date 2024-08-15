using ApacheKafkaBasics.Interfaces;
using Generated.Entity;
using Microsoft.AspNetCore.Mvc;

namespace ApacheKafkaBasics.Controllers;

[ApiController]
[Route("[controller]")]
public class CartController(IShoppingCartRepository shoppingCartRepository) : Controller
{
    [HttpPost("AddToCart")]
    public async Task<IActionResult> AddToCart(Product product, int quantity)
    {
        await shoppingCartRepository.AddProduct(product, quantity);
        return Ok("Successfully Added product: " + product.Name);
    }

    [HttpDelete("RemoveFromCart")]
    public async Task<IActionResult> RemoveFromCart(int productId)
    {
        await shoppingCartRepository.RemoveProduct(productId);
        return Ok("Successfully Added removed product with productId: " + productId);
    }

    [HttpGet("GetTotalPrice")]
    public async Task<IActionResult> GetTotalPrice()
    {
        var totalPrice = await shoppingCartRepository.GetTotalPrice();
        return Ok(totalPrice);
    }

    [HttpGet("GetCartItems")]
    public async Task<IActionResult> GetCartItems()
    {
        var cartItems = await shoppingCartRepository.GetCartItems();
        if (cartItems.Count > 0)
            return Ok(cartItems);
        return NotFound("No item found in the cart");
    }
}