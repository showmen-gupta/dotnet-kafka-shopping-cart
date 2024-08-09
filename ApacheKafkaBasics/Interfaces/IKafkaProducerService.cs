namespace ApacheKafkaBasics.Interfaces;

public interface IKafkaProducerService
{
    public Task ProduceAsync(string topic, string message);

}