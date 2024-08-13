using ApacheKafkaBasics.Models;

namespace ApacheKafkaBasics.Interfaces;

public interface IShoppingCart
{
    public Task<bool> AddProduct(Product product, int quantity);
    public Task<bool> RemoveProduct(int productId);
    public Task<decimal> GetTotalPrice();
    public Task<List<CartItem>> GetCartItems();
}