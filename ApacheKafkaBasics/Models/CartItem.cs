namespace ApacheKafkaBasics.Models;

// CartItem class representing an item in the shopping cart
public class CartItem(Product product, int quantity)
{
    public Product Product { get; set; } = product;
    public int Quantity { get; set; } = quantity;

    // Calculate the total price for this cart item
    public decimal TotalPrice => Product.Price * Quantity;
}