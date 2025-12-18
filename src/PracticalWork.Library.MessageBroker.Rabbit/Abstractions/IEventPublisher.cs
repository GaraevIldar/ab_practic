namespace PracticalWork.Library.MessageBroker.Rabbit.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync<T>(T message, string routingKey)
        where T : IIntegrationEvent;
}