using Microsoft.Extensions.Logging;
using PracticalWork.Library.Abstractions.MessageBroker;
using PracticalWork.Library.Events;
using PracticalWork.Library.MessageBroker.Rabbit.Consumer;

namespace PracticalWork.Library.MessageBroker.Rabbit.Services;

public class RabbitConsumerService
{
    private readonly ILogger<RabbitConsumerService> _logger;
    private readonly IEnumerable<IRabbitMQConsumer> _consumers;

    public RabbitConsumerService(
        ILogger<RabbitConsumerService> logger,
        IEnumerable<IRabbitMQConsumer> consumers)
    {
        _logger = logger;
        _consumers = consumers;
    }

    /// <summary>
    /// Запуск всех подписок
    /// </summary>
    public async Task StartAllAsync()
    {
        foreach (var consumer in _consumers)
        {
            await consumer.BeginListeningAsync(GetQueueName(consumer));
            _logger.LogInformation("Подписка запущена: {Consumer}", consumer.GetType().Name);
        }
    }

    /// <summary>
    /// Остановка всех подписок
    /// </summary>
    public async Task StopAllAsync()
    {
        foreach (var consumer in _consumers)
        {
            await consumer.StopListeningAsync();
            _logger.LogInformation("Подписка остановлена: {Consumer}", consumer.GetType().Name);
        }
    }

    private string GetQueueName(IRabbitMQConsumer consumer)
    {
        return consumer switch
        {
            ReportGenerateConsumer => "report-queue",
            SystemActivityConsumer<BaseEvent> => "system-activity-queue",
            _ => throw new InvalidOperationException("Неизвестный консьюмер")
        };
    }
}