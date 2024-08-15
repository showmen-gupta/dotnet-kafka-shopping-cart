using Generated.Entity;

namespace ApacheKafkaBasics.Interfaces;

public interface IShoppingCartRepository
{
    public Task<bool> AddProduct(Product product, int quantity);
    public Task<bool> RemoveProduct(int productId);
    public Task<double> GetTotalPrice();
    public Task<List<CartItem>> GetCartItems();
}