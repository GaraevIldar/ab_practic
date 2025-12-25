namespace PracticalWork.Library.Abstractions.MessageBroker;

/// <summary>
/// Потребитель очереди сообщений
/// </summary>
/// <summary>
/// Абстрактный потребитель сообщений RabbitMQ
/// </summary>
public interface IRabbitMQConsumer
{
    /// <summary>
    /// Запустить прослушивание очереди и обработку сообщений
    /// </summary>
    /// <param name="queue">Имя очереди</param>
    /// <returns>Асинхронная задача</returns>
    Task BeginListeningAsync(string queue);

    /// <summary>
    /// Прекратить прослушивание очереди
    /// </summary>
    /// <returns>Асинхронная задача</returns>
    Task StopListeningAsync();
}
