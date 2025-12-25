using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PracticalWork.Library.MessageBroker.Rabbit.Abstractions;
using PracticalWork.Library.MessageBroker.Rabbit.Models;
using RabbitMQ.Client;

namespace PracticalWork.Library.MessageBroker.Rabbit;

public class RabbitSetupService
{
    private readonly IRabbitChannelManager _pool;
    private readonly ILogger<RabbitSetupService> _log;
    private readonly IConfiguration _config;

    public IReadOnlyCollection<string> DeclaredQueues { get; private set; } = [];

    public RabbitSetupService(
        IRabbitChannelManager pool,
        ILogger<RabbitSetupService> log,
        IConfiguration config)
    {
        _pool = pool;
        _log = log;
        _config = config;
    }

    public async Task InitializeAsync()
    {
        IChannel channel = null!;
        var resultQueues = new List<string>();

        try
        {
            channel = await _pool.GetChannelAsync();

            var rabbitRoot = _config.GetSection("App:RabbitMQ");

            var libraryCfg = ReadConfig<LibraryConfig>(rabbitRoot, "Library");
            var reportsCfg = ReadConfig<ReportsConfig>(rabbitRoot, "Reports");

            await DeclareLibraryQueues(channel, libraryCfg, resultQueues);
            await DeclareReportsQueue(channel, reportsCfg, resultQueues);

            DeclaredQueues = resultQueues;
            _log.LogInformation("RabbitMQ успешно инициализирован");
        }
        catch (Exception e)
        {
            _log.LogError(e, "Ошибка при инициализации RabbitMQ");
            throw;
        }
        finally
        {
            if (channel != null)
                _pool.ReturnChannel(channel);
        }
    }

    private static T ReadConfig<T>(IConfiguration root, string section)
        where T : new()
    {
        return root.GetSection(section).Get<T>() ?? new T();
    }

    private static async Task DeclareLibraryQueues(
        IChannel channel,
        LibraryConfig config,
        ICollection<string> queues)
    {
        await channel.ExchangeDeclareAsync(
            exchange: config.ExchangeName,
            type: ExchangeType.Direct,
            durable: true);

        var bindings = config.GetType()
            .GetProperties()
            .Where(p => p.PropertyType == typeof(QueueBindingConfig))
            .Select(p => (QueueBindingConfig)p.GetValue(config)!);

        foreach (var binding in bindings)
        {
            await channel.QueueDeclareAsync(
                binding.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            await channel.QueueBindAsync(
                binding.QueueName,
                config.ExchangeName,
                binding.RoutingKey);

            queues.Add(binding.QueueName);
        }
    }

    private static async Task DeclareReportsQueue(
        IChannel channel,
        ReportsConfig config,
        ICollection<string> queues)
    {
        await channel.ExchangeDeclareAsync(
            config.Exchange,
            ExchangeType.Direct,
            durable: true);

        await channel.QueueDeclareAsync(
            config.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        await channel.QueueBindAsync(
            config.QueueName,
            config.Exchange,
            config.RoutingKey);

        queues.Add(config.QueueName);
    }
}