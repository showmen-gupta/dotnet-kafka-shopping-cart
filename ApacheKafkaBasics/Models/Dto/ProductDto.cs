namespace ApacheKafkaBasics.Models.Dto;

public class ProductDto
{
    private static int _productCounter = 0;

    public int ProductId { get; private set; }
    public string Name { get; set; }
    public double Price { get; set; }

    public ProductDto(string name, double price)
    {
        ProductId = ++_productCounter;
        Name = name;
        Price = price;
    }
    
}