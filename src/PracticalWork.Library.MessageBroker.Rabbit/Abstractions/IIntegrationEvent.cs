namespace PracticalWork.Library.MessageBroker.Rabbit.Abstractions;

public interface IIntegrationEvent
{
    DateTime OccurredAt { get; }
}