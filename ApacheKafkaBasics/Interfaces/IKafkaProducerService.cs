using Generated.Entity;

namespace ApacheKafkaBasics.Interfaces;

public interface IKafkaProducerService
{
    public Task CreateKafkaCartTopic(List<CartItem> cartItems);
}