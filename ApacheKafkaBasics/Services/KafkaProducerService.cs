using System.Net;
using ApacheKafkaBasics.Interfaces;
using ApacheKafkaBasics.Models;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;

namespace ApacheKafkaBasics.Services;

public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducer<string, CartItem> _producer;
    private readonly IAdminClient _adminClient;

    public KafkaProducerService(string brokerList)
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
    }

    public async Task CreateKafkaCartTopic(List<CartItem> cartItems, string topic)
    {
        try
        {
            await _adminClient.CreateTopicsAsync(new[]
            {
                new TopicSpecification
                {
                    Name = "in-cart-items",
                    ReplicationFactor = 1,
                    NumPartitions = 3
                }
            });

            foreach (var cartItem in cartItems.Select(item => new CartItem(item.Product, item.Quantity)))
            {
                var result = await _producer.ProduceAsync(topic,
                    new Message<string, CartItem>
                    {
                        Key = $"{cartItem.Product.ProductId}-{DateTime.UtcNow.Ticks}",
                        Value = cartItem
                    });
                
                Console.WriteLine(
                    $"\nMsg: Your leave request is queued at offset {result.Offset.Value} in the Topic {result.Topic}");
            }
        }
        catch (CreateTopicsException e) when (e.Results.Select(r => r.Error.Code)
                                                  .Any(el => el == ErrorCode.TopicAlreadyExists))
        {
            Console.WriteLine($"Topic {e.Results[0].Topic} already exists");
        }
    }
}