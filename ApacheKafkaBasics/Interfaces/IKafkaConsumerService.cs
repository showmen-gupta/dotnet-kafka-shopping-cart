namespace ApacheKafkaBasics.Interfaces;

public interface IKafkaConsumerService
{
    public void StartCartConsumer(CancellationToken cancellationToken);
    public Task<bool> TryDequeueMessage(out string? message);
    public Task<IEnumerable<string>> GetAllMessages();

}