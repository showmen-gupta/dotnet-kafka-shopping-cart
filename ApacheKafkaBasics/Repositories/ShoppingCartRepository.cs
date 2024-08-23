using ApacheKafkaBasics.Interfaces;
using ApacheKafkaBasics.Models.Dto;
using Generated.Entity;

namespace ApacheKafkaBasics.Repositories;

public class ShoppingCartRepositoryRepository : IShoppingCartRepository
{
    private readonly List<CartItem> _items = [];

    public Task<bool> AddProduct(ProductDto productDto, int quantity)
    {
        try
        {
            var existingItem = _items.Find(item => item.Product.ProductId == productDto.ProductId);
            if (existingItem != null)
                // If the product already exists in the cart, increase the quantity
                existingItem.Quantity += quantity;
            else
                // Otherwise, add a new CartItem
                _items.Add(new CartItem
                {
                    Product = new Product
                    {
                        ProductId = productDto.ProductId,
                        Name = productDto.Name,
                        Price = productDto.Price
                    },
                    Quantity = quantity,
                    TotalPrice = productDto.Price * quantity
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

    public Task<List<CartItemDto>> GetCartItems()
    {
        var dtoCartItems = new List<CartItemDto>();
        try
        {
            dtoCartItems.AddRange(from item in _items
                let productDto = new ProductDto(item.Product.Name, item.Product.Price)
                select new CartItemDto(productDto, item.Quantity));
            return Task.FromResult(dtoCartItems);
        }
        catch (Exception ex)
        {
            throw new BadHttpRequestException(ex.Message);
        }
    }
}