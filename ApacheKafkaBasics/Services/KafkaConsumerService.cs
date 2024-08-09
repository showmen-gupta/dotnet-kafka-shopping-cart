using System.Collections.Concurrent;
using ApacheKafkaBasics.Interfaces;
using Confluent.Kafka;

namespace ApacheKafkaBasics.Services;

public class KafkaConsumerService : IKafkaConsumerService
{
    private readonly IConsumer<Null, string> _consumer;
    private readonly ConcurrentQueue<string?> _messageQueue;

    public KafkaConsumerService(string brokerList, string groupId, string topic)
    {
        var config = new ConsumerConfig
        {
            GroupId = groupId,
            BootstrapServers = brokerList,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<Null, string>(config).Build();
        _consumer.Subscribe(topic);
        _messageQueue = new ConcurrentQueue<string?>();
    }


    public void Consume(CancellationToken cancellationToken)
    {
        Task.Run(() =>
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var consumeResult = _consumer.Consume(cancellationToken);
                    _messageQueue.Enqueue(consumeResult.Message.Value);
                    Console.WriteLine(
                        $"Consumed message '{consumeResult.Message.Value}' at: '{consumeResult.TopicPartitionOffset}'.");
                }
            }
            catch (OperationCanceledException)
            {
                _consumer.Close();
            }
        }, cancellationToken);
    }

    public bool TryDequeueMessage(out string? message)
    {
        try
        {
            return _messageQueue.TryDequeue(out message);
        }
        catch (Exception ex)
        {
            throw new BadHttpRequestException(ex.Message);
        }
    }

    public IEnumerable<string> GetAllMessages()
    {
        try
        {
            return _messageQueue.ToArray()!;
        }
        catch (Exception ex)
        {
            throw new BadHttpRequestException(ex.Message);
        }
    }
}