namespace ApacheKafkaBasics.Models;

// CartItem class representing an item in the shopping cart
public class CartItem
{
    public Product Product { get; set; }
    public int Quantity { get; set; }

    public CartItem(Product product, int quantity)
    {
        Product = product;
        Quantity = quantity;
    }

    // Calculate the total price for this cart item
    public decimal TotalPrice => Product.Price * Quantity;
}