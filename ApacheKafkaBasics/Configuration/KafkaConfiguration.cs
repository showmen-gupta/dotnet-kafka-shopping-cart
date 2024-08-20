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
        var kafkaTopic = configuration.GetValue<string>("Kafka:Topic") ?? "in-cart-items";

        services.AddSingleton<KafkaProducerService>(sp => new KafkaProducerService(kafkaBroker, kafkaTopic));
        services.AddSingleton<KafkaConsumerService>(sp =>
            new KafkaConsumerService(kafkaBroker, kafkaGroupId, kafkaTopic));
        services.AddSingleton<IShoppingCartRepository, ShoppingCartRepositoryRepository>();
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

    public static void InitiateKafkaConsumer(KafkaConsumerService kafkaConsumerService,
        CancellationTokenSource? cancellationTokenSource)
    {
        if (cancellationTokenSource != null)
            Task.Run(() => kafkaConsumerService.StartCartConsumer(cancellationTokenSource.Token),
                cancellationTokenSource.Token);
    }
}