using PracticalWork.Library.MessageBroker.Rabbit.Abstractions;

namespace PracticalWork.Library.MessageBroker.Rabbit;

public class RabbitMqEventPublisher : IEventPublisher
{
    private IEventPublisher _eventPublisherImplementation;

    public RabbitMqEventPublisher(IEventPublisher eventPublisherImplementation)
    {
        _eventPublisherImplementation = eventPublisherImplementation;
    }

    public Task PublishAsync<T>(T message, string routingKey) where T : IIntegrationEvent
    {
        return _eventPublisherImplementation.PublishAsync(message, routingKey);
    }
}