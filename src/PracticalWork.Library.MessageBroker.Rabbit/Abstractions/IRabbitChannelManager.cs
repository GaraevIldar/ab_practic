namespace PracticalWork.Library.MessageBroker.Rabbit.Abstractions;
using RabbitMQ.Client;
public interface IRabbitChannelManager : IDisposable
{
    Task<IChannel> GetChannelAsync();
    Task<IChannel> GetChannelForConsumerAsync();
    void ReturnChannel(IChannel channel);
}