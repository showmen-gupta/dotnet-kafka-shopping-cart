namespace ApacheKafkaBasics.Interfaces;

public interface IKafkaConsumerService
{
    public void Consume(CancellationToken cancellationToken);
    public bool TryDequeueMessage(out string? message);
    public IEnumerable<string> GetAllMessages();

}