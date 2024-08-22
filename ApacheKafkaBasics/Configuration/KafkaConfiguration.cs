using ApacheKafkaBasics.Interfaces;
using ApacheKafkaBasics.Repositories;
using ApacheKafkaBasics.Services;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;

namespace ApacheKafkaBasics.Configuration;

public static class KafkaConfiguration
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Add controllers and other services to the container
        services.AddControllers(static options =>
        {
            var formatter = options.InputFormatters.OfType<SystemTextJsonInputFormatter>()
                .First(static formatter => formatter.SupportedMediaTypes.Contains("application/json"));

            formatter.SupportedMediaTypes.Add("application/csp-report");
            formatter.SupportedMediaTypes.Add("application/reports+json");
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Register Kafka services with configurable settings
        services.AddSingleton<KafkaProducerService>(sp =>
        {
            var kafkaConfig = sp.GetRequiredService<IOptions<KafkaConfigSecrets>>().Value;
            return new KafkaProducerService(kafkaConfig.KafkaBroker, kafkaConfig.KafkaTopic, kafkaConfig.SchemaRegistry);
        });

        services.AddSingleton<KafkaConsumerService>(sp =>
        {
            var kafkaConfig = sp.GetRequiredService<IOptions<KafkaConfigSecrets>>().Value;
            return new KafkaConsumerService(kafkaConfig.KafkaBroker, kafkaConfig.KafkaGroupId, kafkaConfig.KafkaTopic, kafkaConfig.SchemaRegistry);
        });

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

    public static async void InitiateKafkaConsumer(KafkaConsumerService kafkaConsumerService,
        CancellationTokenSource? cancellationTokenSource)
    {
        if (cancellationTokenSource != null)
            await Task.WhenAny(Task.Run(() => kafkaConsumerService.StartCartConsumer(cancellationTokenSource.Token),
                cancellationTokenSource.Token), Task.Run(
                () => kafkaConsumerService.StartCartItemProcessor(cancellationTokenSource.Token),
                cancellationTokenSource.Token));
    }
}