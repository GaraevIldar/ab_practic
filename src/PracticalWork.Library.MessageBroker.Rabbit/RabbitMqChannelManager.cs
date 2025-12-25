using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PracticalWork.Library.MessageBroker.Rabbit.Abstractions;
using RabbitMQ.Client;

namespace PracticalWork.Library.MessageBroker.Rabbit;

using System.Collections.Concurrent;

public sealed class RabbitMqChannelManager : IRabbitChannelManager, IInitializable, IDisposable
{
    private readonly ILogger<RabbitMqChannelManager> _log;
    private readonly ConcurrentBag<IChannel> _freeChannels = new();
    private readonly List<IChannel> _consumerChannels = new();
    private readonly SemaphoreSlim _semaphore;
    private readonly ConnectionFactory _connectionFactory;

    private IConnection? _connection;
    private readonly int _poolLimit;
    private readonly string _connectionName;

    public RabbitMqChannelManager(
        IConfiguration configuration,
        ILogger<RabbitMqChannelManager> logger)
    {
        _log = logger;

        _poolLimit = configuration.GetValue("RabbitMQ:MaxChannelPoolSize", 10);
        _semaphore = new SemaphoreSlim(_poolLimit, _poolLimit);

        _connectionName = configuration["RabbitMQ:AppName"]
                          ?? Guid.NewGuid().ToString();

        _connectionFactory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
            UserName = configuration["RabbitMQ:UserName"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest",
            Port = configuration.GetValue("RabbitMQ:Port", 5678),
        };
    }

    public bool IsInit { get; set; }

    public async Task InitializeAsync()
    {
        _connection = await _connectionFactory.CreateConnectionAsync(_connectionName);
        _connection.ConnectionShutdownAsync += (_, args) =>
        {
            _log.LogWarning(
                "RabbitMQ connection closed: {Reason}",
                args.ReplyText);

            return Task.CompletedTask;
        };
    }

    public async Task<IChannel> GetChannelAsync()
    {
        await _semaphore.WaitAsync();

        try
        {
            if (_freeChannels.TryTake(out var cachedChannel) &&
                cachedChannel.IsOpen)
            {
                return cachedChannel;
            }

            EnsureConnection();

            var channel = await _connection!.CreateChannelAsync();
            SubscribeToChannelShutdown(channel);

            return channel;
        }
        catch
        {
            _semaphore.Release();
            throw;
        }
    }

    public async Task<IChannel> GetChannelForConsumerAsync()
    {
        EnsureConnection();

        var channel = await _connection!.CreateChannelAsync();
        _consumerChannels.Add(channel);

        SubscribeToChannelShutdown(channel);

        return channel;
    }

    public void ReturnChannel(IChannel channel)
    {
        try
        {
            if (!channel.IsOpen)
            {
                _log.LogDebug("\nКанал уже закрыт, вызываем dispose");
                channel.Dispose();
                return;
            }

            if (_freeChannels.Count < _poolLimit)
            {
                _freeChannels.Add(channel);
                _log.LogDebug(
                    "Канал возвращен в pool. Текущее количество: {Count}",
                    _freeChannels.Count);
            }
            else
            {
                _log.LogDebug("\nДостигнут лимит, вызываем dispose");
                channel.Dispose();
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void SubscribeToChannelShutdown(IChannel channel)
    {
        channel.ChannelShutdownAsync += (_, args) =>
        {
            _log.LogError(
                "Канал RabbitMQ закрыт: {Reason}",
                args.ReplyText);

            return Task.CompletedTask;
        };
    }

    private void EnsureConnection()
    {
        if (_connection is null)
        {
            throw new InvalidOperationException("\nСоединение RabbitMQ не инициализировано");
        }
    }

    public void Dispose()
    {
        foreach (var channel in _freeChannels)
        {
            if (channel.IsOpen)
            {
                channel.Dispose();
            }
        }

        foreach (var consumerChannel in _consumerChannels)
        {
            if (consumerChannel.IsOpen)
            {
                consumerChannel.Dispose();
            }
        }

        if (_connection is { IsOpen: true })
        {
            _connection.Dispose();
        }

        _freeChannels.Clear();
        _semaphore.Dispose();
    }
}
