using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PracticalWork.Library.Abstractions.MessageBroker;
using PracticalWork.Library.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PracticalWork.Library.MessageBroker.Rabbit.Abstractions;

/// <summary>
/// Базовый класс для потребителя сообщений из RabbitMQ
/// </summary>
/// <typeparam name="TMessage">Тип события для обработки</typeparam>
public abstract class BaseRabbitConsumer<TMessage> : IRabbitMQConsumer where TMessage : BaseEvent
{
    private IChannel? _channel;
    private string? _consumerTag;
    private readonly IRabbitChannelManager _channelManager;
    protected readonly ILogger<BaseRabbitConsumer<TMessage>> Logger;

    protected BaseRabbitConsumer(
        ILogger<BaseRabbitConsumer<TMessage>> logger,
        IRabbitChannelManager channelManager)
    {
        Logger = logger;
        _channelManager = channelManager;
    }

    /// <summary>
    /// Начать прослушивание очереди и обработку сообщений
    /// </summary>
    public async Task BeginListeningAsync(string queueName)
    {
        _channel = await _channelManager.GetChannelForConsumerAsync();
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, ea) => { await ProcessIncomingMessageAsync(ea, queueName); };

        _consumerTag = await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: true,
            consumer: consumer);

        Logger.LogInformation("Прослушивание очереди {Queue} запущено", queueName);
    }

    /// <summary>
    /// Прекратить прослушивание очереди
    /// </summary>
    public async Task StopListeningAsync()
    {
        if (!string.IsNullOrEmpty(_consumerTag) && _channel?.IsOpen == true)
        {
            await _channel.BasicCancelAsync(_consumerTag);
            Logger.LogInformation("Прослушивание очереди завершено");
        }
    }

    private async Task ProcessIncomingMessageAsync(BasicDeliverEventArgs args, string queueName)
    {
        var json = Encoding.UTF8.GetString(args.Body.ToArray());
        var props = args.BasicProperties;

        Logger.LogDebug(
            "Получено сообщение из очереди {Queue}. MessageId={Id}, DeliveryTag={Tag}",
            queueName, props.MessageId, args.DeliveryTag);

        var message = JsonSerializer.Deserialize<TMessage>(json);
        if (_channel == null)
            throw new InvalidOperationException("RabbitMQ канал не инициализирован");

        await HandleMessageAsync(message);

        Logger.LogDebug("Сообщение успешно обработано: {MessageId}", props.MessageId);
    }

    /// <summary>
    /// Метод для обработки конкретного события
    /// </summary>
    protected abstract Task HandleMessageAsync(TMessage? message);
}