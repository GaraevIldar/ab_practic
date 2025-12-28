using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PracticalWork.Library.Abstractions.MessageBroker;
using PracticalWork.Library.MessageBroker.Rabbit.Abstractions;
using RabbitMQ.Client;

namespace PracticalWork.Library.MessageBroker.Rabbit.Publishers;

public class RabbitPublisher: IRabbitPublisher
{
    private readonly IRabbitChannelManager _pool;
    private readonly ILogger<RabbitPublisher> _log;

    public RabbitPublisher(
        IRabbitChannelManager pool,
        ILogger<RabbitPublisher> log)
    {
        _pool = pool;
        _log = log;
    }

    public async Task<bool> PublishAsync<T>(
        string exchangeName,
        string key,
        T payload)
    {
        IChannel channel = null;

        try
        {
            channel = await _pool.GetChannelAsync();

            var messageBytes = EncodeMessage(payload);

            await channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: key,
                body: messageBytes);

            return true;
        }
        catch (Exception ex)
        {
            _log.LogError(ex,
                "Не удалось отправить сообщение в RabbitMQ ({Exchange}:{Key})",
                exchangeName, key);
            return false;
        }
        finally
        {
            if (channel != null)
                _pool.ReturnChannel(channel);
        }
    }

    private static byte[] EncodeMessage<T>(T data)
    {
        var json = JsonSerializer.Serialize(data);
        return Encoding.UTF8.GetBytes(json);
    }
}