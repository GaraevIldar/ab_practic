using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Events;
using PracticalWork.Library.MessageBroker.Rabbit.Abstractions;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.MessageBroker.Rabbit.Consumer;

public class SystemActivityConsumer<TEvent> : BaseRabbitConsumer<TEvent> 
    where TEvent : BaseEvent
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SystemActivityConsumer(
        ILogger<BaseRabbitConsumer<TEvent>> logger,
        IRabbitChannelManager channelManager,
        IServiceScopeFactory serviceScopeFactory)
        : base(logger, channelManager)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task HandleMessageAsync(TEvent message)
    {
        if (message == null)
        {
            Logger.LogWarning("Поступило пустое событие системы");
            return;
        }

        if (message.Source != "library-service")
        {
            Logger.LogWarning("Сообщение отклонено: ожидается Source=library-service, получено Source={Source}. EventType={EventType}", message.Source, message.EventType);
            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();

        var activityLog = new ActivityLog
        {
            Event = message,
            EventDate = message.OccurredOn,
            EventType = message.EventType
        };

        try
        {
            await reportService.SaveActivityLogs(activityLog);
            Logger.LogInformation("Лог активности записан в БД: {EventType}, EventId={EventId}", message.EventType, message.EventId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Ошибка сохранения лога активности в БД. EventType={EventType}, EventId={EventId}", message.EventType, message.EventId);
            throw;
        }
    }
}
