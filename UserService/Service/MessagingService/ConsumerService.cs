using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace UserService.Service.MessagingService;

public class ConsumerService : BackgroundService
{
    private readonly ILogger<ConsumerService> _logger;
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly KafkaTopic _topicName;

    public ConsumerService(
        ILogger<ConsumerService> logger,
        IOptions<ConsumerConfig> config,
        KafkaTopic topicName
    )
    {
        _logger = logger;
        _topicName = topicName;
        _consumer = new ConsumerBuilder<Ignore, string>(config.Value).Build();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_topicName.ToString());

        Task.Run(
            () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(5));
                        if (consumeResult == null)
                            continue;

                        var result = JsonConvert.DeserializeObject(
                            consumeResult.Message.Value,
                            TopicTypeMap.Map.GetValueOrDefault(_topicName)
                        );
                        _logger.LogInformation(
                            $"Consumed message '{consumeResult.Message.Value}' at: '{consumeResult.Offset}'"
                        );
                        // You can further process the message `result` here as needed
                    }
                    catch (OperationCanceledException)
                    {
                        // Ignore
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error consuming message: {ex.Message}");
                    }
                }
            },
            stoppingToken
        );

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}
