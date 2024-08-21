using Generated.Entity;

namespace ApacheKafkaBasics.Interfaces;

public interface IKafkaConsumerService
{
    public void StartCartConsumer(CancellationToken cancellationToken);
    public Task StartCartItemProcessor(CancellationToken cancellationToken);

    public Task SendCartItemsToProcess(CartItem cartItemRequest, bool isApproved,
        int? partitionId);

    public Task<IEnumerable<string>> GetAllProcessedMessages();
}