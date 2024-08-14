using ApacheKafkaBasics.Models;

namespace ApacheKafkaBasics.Interfaces;

public interface IKafkaProducerService
{
    public Task CreateKafkaCartTopic(List<CartItem> cartItems, string topic);
}