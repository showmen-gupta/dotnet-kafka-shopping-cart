namespace ApacheKafkaBasics.Models;

public abstract class Product(int productId, string name, decimal price)
{
    public int ProductId { get; set; } = productId;
    public string Name { get; set; } = name;
    public decimal Price { get; set; } = price;
}