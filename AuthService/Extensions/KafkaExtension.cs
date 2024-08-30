
using AuthService.Service.MessagingService;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace AuthService.Extensions;

public static class KafkaExtensions
{
    public static IServiceCollection AddKafkaServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Producer
        services.Configure<ProducerConfig>(configuration.GetSection("KafkaConfig:Producer"));
        services.AddSingleton<ProducerService>();

        // Configure Consumer
        services.Configure<ConsumerConfig>(configuration.GetSection("KafkaConfig:Consumer"));
        services.AddHostedService(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<ConsumerService>>();
            var consumerConfig = provider.GetRequiredService<IOptions<ConsumerConfig>>();
            var topic = KafkaTopic.TestTopic;

            return new ConsumerService(logger, consumerConfig, topic);
        });

        return services;
    }
}