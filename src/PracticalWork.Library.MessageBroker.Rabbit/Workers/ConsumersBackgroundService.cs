using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PracticalWork.Library.Abstractions.MessageBroker;
using PracticalWork.Library.MessageBroker.Rabbit.Abstractions;
using PracticalWork.Library.MessageBroker.Rabbit.Services;

namespace PracticalWork.Library.MessageBroker.Rabbit.Workers;

public class ConsumersBackgroundService: BackgroundService
{
    private readonly RabbitSetupService _setupService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly List<IRabbitMQConsumer> _consumers;
    private readonly IInitializable _initializable;
    private readonly ILogger<ConsumersBackgroundService> _logger;

    public ConsumersBackgroundService(
        RabbitSetupService setupService,
        IServiceScopeFactory factory,
        IInitializable init,
        ILogger<ConsumersBackgroundService> logger)
    {
        _setupService = setupService;
        _consumers = new List<IRabbitMQConsumer>();
        _scopeFactory = factory;
        _initializable = init;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_initializable != null)
        {
            await _initializable.InitializeAsync();
        }
        using var scope = _scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;
        await _setupService.InitializeAsync();

        var queues = _setupService.DeclaredQueues!;

        foreach (var queue in queues)
        {
            var consumer = sp.GetKeyedService<IRabbitMQConsumer>(queue);
            if (consumer == null)
            {
                _logger.LogWarning("Консьюмер для очереди {Queue} не зарегистрирован. Проверьте конфигурацию App:RabbitMQ", queue);
                continue;
            }
            await consumer.BeginListeningAsync(queue);
            _consumers.Add(consumer);
        }
    }
    
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var consumer in _consumers.OfType<IRabbitMQConsumer>())
        {
            await consumer.StopListeningAsync();
        }
        await base.StopAsync(cancellationToken);
    }
}