namespace ApacheKafkaBasics.Models;

public class Product
{
    public int ProductId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public decimal Price { get; set; } = default!;
}