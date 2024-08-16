using ApacheKafkaBasics.Interfaces;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Generated.Entity;
using Microsoft.AspNetCore.Connections;

namespace ApacheKafkaBasics.Services;

public class KafkaConsumerService : IKafkaConsumerService
{
    private readonly IConsumer<string, CartItem> _consumer;
    private readonly ConsumerConfig _consumerConfig;

    private record KafkaMessage(string? Key, int? Partition, CartItem Message);

    public KafkaConsumerService(string brokerList, string groupId, string topic)
    {
        var schemaRegistryConfig = new SchemaRegistryConfig { Url = "http://127.0.0.1:8081" };

        _consumerConfig = new ConsumerConfig
        {
            BootstrapServers = brokerList,
            GroupId = groupId,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false,
            SessionTimeoutMs = 10000, // 10 seconds
            // Read messages from start if no commit exists.
            AutoOffsetReset = AutoOffsetReset.Earliest,
            MaxPollIntervalMs = 500000
        };
        var schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig);

        _consumer = new ConsumerBuilder<string, CartItem>(_consumerConfig)
            .SetKeyDeserializer(new AvroDeserializer<string>(schemaRegistryClient).AsSyncOverAsync())
            .SetValueDeserializer(new AvroDeserializer<CartItem>(schemaRegistryClient).AsSyncOverAsync())
            .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
            .Build();

        _consumer.Subscribe(topic);
    }

    public void StartCartConsumer(CancellationToken cancellationToken)
    {
        Task.Run(() =>
        {
            try
            {
                var cartItemMessages = new Queue<KafkaMessage>();
                while (!cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("Consumer loop started...\n");
                    var result =
                        _consumer.Consume(cancellationToken);
                    var cartRequest = result?.Message?.Value;
                    if (cartRequest == null) continue;

                    var key = result?.Message?.Key;
                    var partition = result?.Partition.Value;

                    cartItemMessages.Enqueue(new KafkaMessage(key, partition, cartRequest));
                    Console.WriteLine(cartItemMessages.Count);
                    _consumer.Commit(result);
                    _consumer.StoreOffset(result);
                }
            }
            catch (Exception ex)
            {
                _consumer.Close();
                throw new ConnectionAbortedException(ex.Message);
            }
        }, cancellationToken);
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