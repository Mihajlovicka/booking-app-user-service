using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace UserService.Service.MessagingService
{
    public class ProducerService : IDisposable
    {
        private readonly ILogger<ProducerService> _logger;
        private readonly IProducer<Null, string> _producer;

        public ProducerService(
            ILogger<ProducerService> logger,
            IOptions<ProducerConfig> kafkaConfig
        )
        {
            _logger = logger;

            _producer = new ProducerBuilder<Null, string>(kafkaConfig.Value)
            //.SetValueSerializer(new JsonSerializer<T>())
            .Build();
        }

        public virtual async Task ProduceAsync<T>(string topic, T message)
        {
            try
            {
                var deliveryResult = await _producer.ProduceAsync(
                    topic,
                    new Message<Null, string> { Value = JsonConvert.SerializeObject(message) }
                );

                _logger.LogInformation(
                    $"Delivered message to {deliveryResult.Value}, Offset: {deliveryResult.Offset}"
                );
            }
            catch (ProduceException<Null, string> e)
            {
                _logger.LogError($"Delivery failed: {e.Error.Reason}");
            }
        }

        public void Dispose()
        {
            _producer.Dispose();
        }
    }
}
