using ApacheKafkaBasics.Interfaces;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Generated.Entity;

namespace ApacheKafkaBasics.Services;

public class KafkaConsumerService : IKafkaConsumerService
{
    private readonly IConsumer<string, CartItem> _consumer;
    private readonly ConsumerConfig _consumerConfig;

    private record KafkaMessage(string? Key, int? Partition, CartItem Message);

    public KafkaConsumerService(string brokerList, string groupId, string topic)
    {
        var schemaRegistryConfig = new SchemaRegistryConfig { Url = brokerList };

        _consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "127.0.0.1:9092",
            GroupId = groupId,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false,
            // Read messages from start if no commit exists.
            AutoOffsetReset = AutoOffsetReset.Earliest,
            MaxPollIntervalMs = 10000
        };
        var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);

        _consumer = new ConsumerBuilder<string, CartItem>(_consumerConfig)
            .SetKeyDeserializer(new AvroDeserializer<string>(schemaRegistry).AsSyncOverAsync())
            .SetValueDeserializer(new AvroDeserializer<CartItem>(schemaRegistry).AsSyncOverAsync())
            .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
            .Build();

        _consumer.Subscribe(topic);
    }


    public Task StartCartConsumer(CancellationToken cancellationToken)
    {
        var cartItemMessages = new Queue<KafkaMessage>();
        
        if (cartItemMessages == null) throw new BadHttpRequestException("There are no values on the queue");

        Console.WriteLine("Consumer loop started...\n");
        
        while (true)
            try
            {
                var result =
                    _consumer.Consume(
                        TimeSpan.FromMilliseconds(_consumerConfig.MaxPollIntervalMs - 1000 ?? 250000));
                var cartRequest = result?.Message?.Value;
                if (cartRequest == null) continue;

                var key = result?.Message?.Key;
                var partition = result?.Partition.Value;

                // Adding message to a list just for the demo.
                // You should persist the message in database and process it later.
                cartItemMessages.Enqueue(new KafkaMessage(key,
                    partition, cartRequest));

                _consumer.Commit(result);
                _consumer.StoreOffset(result);
            }
            catch (ConsumeException e) when (!e.Error.IsFatal)
            {
                Console.WriteLine($"Non fatal error: {e}");
            }

            finally
            {
                _consumer.Close();
            }
    }

    public Task<bool> TryDequeueMessage(out string? message)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<string>> GetAllMessages()
    {
        throw new NotImplementedException();
    }
}