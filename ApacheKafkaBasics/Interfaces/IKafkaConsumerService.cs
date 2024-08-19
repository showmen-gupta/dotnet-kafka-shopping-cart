using Generated.Entity;

namespace ApacheKafkaBasics.Interfaces;

public interface IKafkaConsumerService
{
    public Task StartCartConsumer(CancellationToken cancellationToken);
    public Task StartCartItemProcessor(bool isApproved, CancellationToken cancellationToken);

    public Task SendCartItemsToProcess(CartItem cartItemRequest, bool isApproved,
        int? partitionId);

    public Task<IEnumerable<string>> GetAllMessages();
}