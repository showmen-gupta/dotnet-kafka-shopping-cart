using ApacheKafkaBasics.Interfaces;
using ApacheKafkaBasics.Repositories;
using ApacheKafkaBasics.Services;

namespace ApacheKafkaBasics.Configuration;

public static class KafkaConfiguration
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Add controllers and other services to the container
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Register Kafka services with configurable settings
        var kafkaBroker = configuration.GetValue<string>("Kafka:Broker") ?? "localhost:9092";
        var kafkaGroupId = configuration.GetValue<string>("Kafka:GroupId") ?? "in-cart-group";
        var kafkaTopic = configuration.GetValue<string>("Kafka:Topic") ?? "cart-topic";

        services.AddSingleton<KafkaProducerService>(sp => new KafkaProducerService(kafkaBroker));
        services.AddSingleton<KafkaConsumerService>(sp =>
            new KafkaConsumerService(kafkaBroker, kafkaGroupId, kafkaTopic));
        services.AddSingleton<IShoppingCart, ShoppingCart>();
    }

    public static void ConfigureMiddleware(WebApplication application)
    {
        // Only use Swagger in development
        if (application.Environment.IsDevelopment())
        {
            application.UseSwagger();
            application.UseSwaggerUI();
        }

        application.UseHttpsRedirection();
        application.UseAuthorization();
        application.MapControllers();
    }

    public static void StartKafkaConsumer(IServiceProvider services)
    {
        // Start Kafka consumer in a background task
        var kafkaConsumerService = services.GetRequiredService<KafkaConsumerService>();
        var cancellationTokenSource = new CancellationTokenSource();
        Task.Run(() => kafkaConsumerService.Consume(cancellationTokenSource.Token), cancellationTokenSource.Token);
    }
}