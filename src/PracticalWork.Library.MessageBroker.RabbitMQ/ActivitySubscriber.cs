using Microsoft.Extensions.Hosting;
using MessageBroker.RabbitMQ.Data.PostgreSql.Entities;
using MessageBroker.RabbitMQ.Data.PostgreSql.Repository;
using System.Text.Json;

public class ActivitySubscriber : BackgroundService
{
    private readonly IActivityLogRepository _repository;

    public ActivitySubscriber(IActivityLogRepository repository)
    {
        _repository = repository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var fakeMessage = new
            {
                EventType = "BookCreated",
                Payload = "{}"
            };

            var log = new ActivityLog
            {
                Id = Guid.NewGuid(),
                EventType = fakeMessage.EventType,
                Payload = JsonSerializer.Serialize(fakeMessage),
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(log);

            await Task.Delay(5000, stoppingToken);
        }
    }
}
