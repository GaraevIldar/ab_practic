namespace PracticalWork.Library.MessageBroker.Rabbit.Models;

public class QueueBindingConfig
{
    public string QueueName { get; set; } = default!;
    public string RoutingKey { get; set; } = default!;
}