using Generated.Entity;

namespace ApacheKafkaBasics.Interfaces;

public interface IKafkaConsumerService
{
    public Task StartCartConsumer(CancellationToken cancellationToken);
    public Task<bool> TryDequeueMessage(out string? message);
    public Task<IEnumerable<string>> GetAllMessages();
    
    public Task SendMessageToResultTopicAsync(CartItem cartItemRequest, bool isApproved,
        int partitionId
    );

}