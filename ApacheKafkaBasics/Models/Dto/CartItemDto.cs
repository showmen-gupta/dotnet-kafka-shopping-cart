namespace ApacheKafkaBasics.Models.Dto;

public class CartItemDto(ProductDto productDto, int quantity)
{
    public ProductDto ProductDto { get; set; } = productDto;
    public int Quantity { get; set; } = quantity;

    // Calculate the total price for this cart item
    public double TotalPrice => ProductDto.Price * Quantity;
}