using System.Net;
using System.Text.Json;
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
    private readonly IProducer<string, CartItemProcessed> _producer;
    private readonly string _topicName;
    private static Queue<KafkaMessage> _cartItemMessages = new();
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
        var cachedSchemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig);


        _consumer = new ConsumerBuilder<string, CartItem>(_consumerConfig)
            .SetKeyDeserializer(new AvroDeserializer<string>(schemaRegistryClient).AsSyncOverAsync())
            .SetValueDeserializer(new AvroDeserializer<CartItem>(schemaRegistryClient).AsSyncOverAsync())
            .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
            .Build();


        var producerConfig = new ProducerConfig
        {
            BootstrapServers = brokerList,
            // Guarantees delivery of message to topic.
            EnableDeliveryReports = true,
            ClientId = Dns.GetHostName()
        };

        _producer = new ProducerBuilder<string, CartItemProcessed>(producerConfig)
            .SetKeySerializer(new AvroSerializer<string>(cachedSchemaRegistryClient))
            .SetValueSerializer(new AvroSerializer<CartItemProcessed>(cachedSchemaRegistryClient))
            .Build();

        _topicName = topic;
    }

    public void StartCartConsumer(CancellationToken cancellationToken)
    {
        try
        {
            _consumer.Subscribe(_topicName);
            _cartItemMessages = new Queue<KafkaMessage>();

            while (!cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine("Consumer loop started...\n");
                var result =
                    _consumer.Consume(TimeSpan.FromMilliseconds(_consumerConfig.MaxPollIntervalMs - 1000 ?? 250000));

                if (result?.Message?.Value == null) continue;
                var cartRequest = result.Message.Value;
                var key = result.Message.Key;
                var partition = result.Partition.Value;

                _cartItemMessages.Enqueue(new KafkaMessage(key, partition, cartRequest));
                Console.WriteLine(_cartItemMessages.Count);
                _consumer.Commit(result);
                _consumer.StoreOffset(result);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Consumer exception: {ex.Message}");
            throw; // Let the exception bubble up after logging it
        }
        finally
        {
            _consumer?.Close(); // Ensure consumer is closed cleanly
        }
    }

    public async Task StartCartItemProcessor(int productId, bool isApproved, CancellationTokenSource cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_cartItemMessages.Count == 0)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    continue;
                }

                //TODO: need to dequeue the message from the queue too.
                // Loop through the queue to find the specific product
                var targetMessage =
                    _cartItemMessages.FirstOrDefault(message => message.Message.Product.ProductId == productId);


                if (targetMessage == null) throw new BadHttpRequestException("Product not found in the queue");

                // Now remove the target message from the queue
                _cartItemMessages = new Queue<KafkaMessage>(_cartItemMessages.Where(m => m != targetMessage));

                // Process the message
                var (key, partition, cartItem) = targetMessage;

                Console.WriteLine(
                    $"Received message: {key} from partition: {partition} Value: {JsonSerializer.Serialize(cartItem)}");

                // Make decision on queued cart items.
                await SendCartItemsToProcess(cartItem, isApproved, partition);
            }
        }
        catch (Exception ex)
        {
            throw new BadHttpRequestException(ex.Message);
        }
    }

    public async Task SendCartItemsToProcess(CartItem cartItemRequest, bool isApproved, int? partitionId)
    {
        try
        {
            var cartItemResult = new CartItemProcessed
            {
                Product = cartItemRequest.Product,
                Quantity = cartItemRequest.Quantity,
                TotalPrice = cartItemRequest.TotalPrice,
                ProcessedBy = $"Admin #{partitionId}",
                Result = isApproved
                    ? "Approved: Your cart item has been approved to deliver."
                    : "Declined: Your cart item has been declined to be processed."
            };

            var result = await _producer.ProduceAsync(_topicName,
                new Message<string, CartItemProcessed>
                {
                    Key = $"{cartItemRequest.Product.Name}-{DateTime.UtcNow.Ticks}",
                    Value = cartItemResult
                });

            Console.WriteLine(
                $"\nMsg: Your cart request is queued at offset {result.Offset.Value} in the Topic {result.Topic}");
        }
        catch (Exception ex)
        {
            throw new BadHttpRequestException(ex.Message);
        }
    }

    public Task<IEnumerable<string>> GetAllProcessedMessages()
    {
        throw new NotImplementedException();
    }
}