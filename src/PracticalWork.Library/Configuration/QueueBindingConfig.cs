namespace PracticalWork.Library.Configuration;

/// <summary>
/// Настройки очереди и ключа маршрутизации для RabbitMQ
/// </summary>
public class QueueBindingConfig
{
    public string QueueName { get; set; } = default!;
    public string RoutingKey { get; set; } = default!;
}
