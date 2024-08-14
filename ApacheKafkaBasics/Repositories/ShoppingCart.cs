using ApacheKafkaBasics.Interfaces;
using Generated.Entity;

namespace ApacheKafkaBasics.Repositories;

public class ShoppingCart : IShoppingCart
{
    private readonly List<CartItem> _items = [];

    public Task<bool> AddProduct(Product product, int quantity)
    {
        try
        {
            var existingItem = _items.Find(item => item.Product.ProductId == product.ProductId);
            if (existingItem != null)
                // If the product already exists in the cart, increase the quantity
                existingItem.Quantity += quantity;
            else
                // Otherwise, add a new CartItem
                _items.Add(new CartItem
                {
                    Product = product,
                    Quantity = quantity,
                    TotalPrice = product.Price * quantity
                });

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            throw new BadHttpRequestException(ex.Message);
        }
    }

    public Task<bool> RemoveProduct(int productId)
    {
        try
        {
            _items.RemoveAll(item => item.Product.ProductId == productId);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            throw new BadHttpRequestException(ex.Message);
        }
    }

    public Task<double> GetTotalPrice()
    {
        try
        {
            var total = _items.Sum(item => item.Product.Price * item.Quantity);
            return Task.FromResult(total);
        }
        catch (Exception ex)
        {
            throw new BadHttpRequestException(ex.Message);
        }
    }

    public Task<List<CartItem>> GetCartItems()
    {
        try
        {
            return Task.FromResult(_items);
        }
        catch (Exception ex)
        {
            throw new BadHttpRequestException(ex.Message);
        }
    }
}