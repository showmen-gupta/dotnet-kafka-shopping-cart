using ApacheKafkaBasics.Models.Dto;
using Generated.Entity;

namespace ApacheKafkaBasics.Interfaces;

public interface IShoppingCartRepository
{
    public Task<bool> AddProduct(ProductDto productDto, int quantity);
    public Task<bool> RemoveProduct(int productId);
    public Task<double> GetTotalPrice();
    public Task<List<CartItemDto>> GetCartItems();
}