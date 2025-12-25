using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Events;
using PracticalWork.Library.MessageBroker.Rabbit.Abstractions;

namespace PracticalWork.Library.MessageBroker.Rabbit.Consumer;

public class ReportGenerateConsumer : BaseRabbitConsumer<ReportCreateEvent>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ReportGenerateConsumer(
        ILogger<BaseRabbitConsumer<ReportCreateEvent>> logger,
        IRabbitChannelManager channelManager,
        IServiceScopeFactory serviceScopeFactory)
        : base(logger, channelManager)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// Обработка события создания отчета
    /// </summary>
    protected override async Task HandleMessageAsync(ReportCreateEvent? message)
    {
        if (message == null)
        {
            Logger.LogWarning("Поступило пустое сообщение для обработки отчета");
            return;
        }

        if (message.Source != "report-service")
        {
            Logger.LogError(
                "Сообщение отклонено: источник не соответствует ожидаемому");
            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();

        await reportService.GenerateReport(
            message.Id,
            message.PeriodFrom,
            message.PeriodTo,
            message.EventTypes.ToArray());
    }
}
