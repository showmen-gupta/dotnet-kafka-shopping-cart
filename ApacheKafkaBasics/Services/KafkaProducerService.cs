using System.Net;
using ApacheKafkaBasics.Interfaces;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Confluent.SchemaRegistry;

namespace ApacheKafkaBasics.Services;

public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducer<Null, string> _producer;

    public KafkaProducerService(string brokerList)
    {
        var config = new ProducerConfig { BootstrapServers = brokerList };
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }
    
    public async Task CreateKafkaCartTopic()
    {
        var adminConfig = new AdminClientConfig { BootstrapServers = "127.0.0.1:9092" };
        var schemaRegistryConfig = new SchemaRegistryConfig { Url = "http://127.0.0.1:8081" };
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = "127.0.0.1:9092",
            // Guarantees delivery of message to topic.
            EnableDeliveryReports = true,
            ClientId = Dns.GetHostName()
        };
        using var adminClient = new AdminClientBuilder(adminConfig).Build();

        try
        {
            await adminClient.CreateTopicsAsync(new[]
            {
                new TopicSpecification
                {
                    Name = "in-cart-items",
                    ReplicationFactor = 1,
                    NumPartitions = 3
                }
            });
        }
        catch (CreateTopicsException e) when (e.Results.Select(r => r.Error.Code)
                                                  .Any(el => el == ErrorCode.TopicAlreadyExists))
        {
            Console.WriteLine($"Topic {e.Results[0].Topic} already exists");
        }
    }

    public async Task ProduceAsync(string topic, string message)
    {
        try
        {
            var result = await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
            Console.WriteLine($"Delivered '{result.Value}' to '{result.TopicPartitionOffset}'");
        }
        catch (ProduceException<Null, string> e)
        {
            Console.WriteLine($"Delivery failed: {e.Error.Reason}");
        }
    }
}