using System.Net;
using ApacheKafkaBasics.Interfaces;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Generated.Entity;

namespace ApacheKafkaBasics.Services;

public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducer<string, CartItem> _producer;
    private readonly IAdminClient _adminClient;
    private readonly string _topicName;

    public KafkaProducerService(string brokerList, string kafkaTopic)
    {
        var adminConfig = new AdminClientConfig { BootstrapServers = brokerList };
        var schemaRegistryConfig = new SchemaRegistryConfig { Url = "http://127.0.0.1:8081" };
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = "127.0.0.1:9092",
            // Guarantees delivery of message to topic.
            EnableDeliveryReports = true,
            ClientId = Dns.GetHostName()
        };

        var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);

        _adminClient = new AdminClientBuilder(adminConfig).Build();

        _producer = new ProducerBuilder<string, CartItem>(producerConfig)
            .SetKeySerializer(new AvroSerializer<string>(schemaRegistry))
            .SetValueSerializer(new AvroSerializer<CartItem>(schemaRegistry))
            .Build();

        _topicName = kafkaTopic;
    }

    public async Task CreateKafkaCartTopic(List<CartItem> cartItems)
    {
        try
        {
            // Check if the topic already exists
            var metadata = _adminClient.GetMetadata(_topicName, TimeSpan.FromSeconds(10));
            var topicExists = metadata.Topics.Any(t => t.Topic == _topicName);

            // Create the topic only if it doesn't exist
            if (!topicExists)
                await _adminClient.CreateTopicsAsync(new[]
                {
                    new TopicSpecification
                    {
                        Name = _topicName,
                        ReplicationFactor = 1,
                        NumPartitions = 3
                    }
                });

            // Produce messages to the topic
            foreach (var cartValues in cartItems.Select(cartItem => new CartItem
                     {
                         Product = cartItem.Product,
                         Quantity = cartItem.Quantity,
                         TotalPrice = cartItem.TotalPrice
                     }))
            {
                var result = await _producer.ProduceAsync(_topicName,
                    new Message<string, CartItem>
                    {
                        Key = $"{cartValues.Product.Name}-{DateTime.UtcNow.Ticks}",
                        Value = cartValues
                    });

                Console.WriteLine(
                    $"\nMsg: Your cart request is queued at offset {result.Offset.Value} in the Topic {result.Topic}");
            }
        }
        catch (CreateTopicsException e)
        {
            if (e.Results.Select(r => r.Error.Code).Any(el => el == ErrorCode.TopicAlreadyExists))
                // Log the fact that the topic already exists
                Console.WriteLine($"Topic {_topicName} already exists.");
            else
                throw;
        }
        catch (Exception ex)
        {
            // Handle other exceptions
            Console.WriteLine($"An error occurred: {ex.Message}");
            throw;
        }
    }
    public Task<IEnumerable<string>> GetAllQueuedMessages()
    {
        throw new NotImplementedException();
    }
}