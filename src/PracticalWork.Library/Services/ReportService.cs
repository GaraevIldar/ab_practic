using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PracticalWork.Library.Abstractions.MessageBroker;
using PracticalWork.Library.Abstractions.Minio;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v1.Pagination;
using PracticalWork.Library.Contracts.v1.Report;
using PracticalWork.Library.Data.Minio;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Events;
using PracticalWork.Library.Exceptions.Report;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Services;

public class ReportService: IReportService
{
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IActivityLogPaginationService _activeLogPaginationService;
    private readonly IReportRepository _reportRepository;
    private readonly IRabbitPublisher _publisher;
    private readonly IMinioService _minioService;
    private readonly MinioOptions _minioOptions;
    private readonly ICacheService _cacheService;
    private readonly string _exchangeName;
    private readonly string _routingKey;
    private readonly string _cacheVersion;
    private readonly string _reportsListPrefix;
    private readonly double _reportsListTtlInMinutes;

    private readonly TimeProvider _timeProvider;

    public ReportService(IActivityLogRepository activityLogRepository,
        IActivityLogPaginationService activeLogPaginationService,
        IReportRepository reportRepository,
        IRabbitPublisher publisher,
        IConfiguration configuration,
        ICacheService cacheService,
        IMinioService minioService,
        IOptionsMonitor<MinioOptions> minioOptions,
        TimeProvider timeProvider)
    {
        _activityLogRepository = activityLogRepository;
        _activeLogPaginationService = activeLogPaginationService;
        _reportRepository = reportRepository;
        _cacheService = cacheService;
        _minioService = minioService;
        _minioOptions = minioOptions.CurrentValue;
        _publisher = publisher;
        _timeProvider = timeProvider;
        var rabbitSection = configuration.GetSection("App:RabbitMQ:Reports");
        _exchangeName = rabbitSection["Exchange"];
        _routingKey = rabbitSection["RoutingKey"];
        var redisSection = configuration.GetSection("App:Redis:Reports");
        _cacheVersion = redisSection["VersionKey"];
        _reportsListPrefix = redisSection["ReportsList:Prefix"];
        _reportsListTtlInMinutes = redisSection.GetValue<double>("ReportsList:TtlInMinutes");
    }
    public async Task SaveActivityLogs(ActivityLog log)
    {
        await _activityLogRepository.AddLogAsync(log);
    }

    public async Task<IReadOnlyList<ActivityLog>> GetActivityLogs(ActivityLogsPaginationRequest request)
    {
        var logs = await _activityLogRepository.GetLogsPageAsync(request);
        return _activeLogPaginationService.PaginationLogs(logs, request.PageNumber, request.PageSize);
    }


    public async Task<Report> CreateReport(DateOnly? dateFrom, DateOnly? dateTo, string[] eventTypes)
    {
        var report = new Report
        {
            PeriodFrom = dateFrom,
            PeriodTo = dateTo,
            EventTypes = eventTypes,
        };
        var id = await _reportRepository.CreateReport(report);
        var message = new ReportCreateEvent(id, dateFrom, dateTo, eventTypes, report.Status);
        await _publisher.PublishAsync(_exchangeName, _routingKey, message);
        await _cacheService.InvalidateCache(_cacheVersion);
        return report;
    }

    public async Task GenerateReport(Guid reportId, DateOnly? periodFrom, DateOnly? periodTo, string[] eventTypes)
    {
        var report = await _reportRepository.GetReportById(reportId);
        
        if (report == null)
            throw new ReportNotFoundException(reportId);
        
        var logs = await _activityLogRepository.GetLogsAsync(
            periodFrom, periodTo, eventTypes);
        try
        {
            var reportResult = GenerateReport(reportId, logs);
            await _minioService.UploadFileAsync(reportResult.FileName, reportResult.Content, reportResult.ContentType);
            var fileName = reportResult.FileName.Split('/')[^1];
            report.MakeGenerated(fileName);
            await _reportRepository.UpdateReport(reportId,report);
            await _cacheService.InvalidateCache(_cacheVersion);
        }
        catch (Exception)
        {
            report.Status = ReportStatus.Error;
            await _reportRepository.UpdateReport(reportId,report);
            await _cacheService.InvalidateCache(_cacheVersion);
            throw;
        }
    }
    public ReportGenerateResult GenerateReport(Guid reportId, IReadOnlyList<ActivityLog> logs)
    {
        var timestamp = _timeProvider.GetUtcNow().UtcDateTime;
        string fileName = $"{timestamp.Year}/{timestamp.Month}/{reportId}.csv";
        string contentType = "text/csv";

        var sb = new StringBuilder();
        
        sb.AppendLine("EventType;EventDate;Metadata");
        foreach (var log in logs)
        {
            sb.AppendLine($"{log.EventType};{log.EventDate};{JsonSerializer.Serialize(log, log.GetType())}");
        }
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));

        return new ReportGenerateResult(stream, contentType,fileName);
    }

    public async Task<IReadOnlyList<Report>> GetListReadyReports()
    {
        var cacheVersion = await _cacheService.GetCurrentCacheVersion(_cacheVersion);
        var cacheKey = _cacheService.GenerateCacheKey(_reportsListPrefix, cacheVersion, null);
        var cachedResult = await _cacheService.GetAsync<IReadOnlyList<Report>>(cacheKey);
        
        if (cachedResult != null)
            return cachedResult;
        
        var reports = await _reportRepository.GetReadyReports();

        await _cacheService.SetAsync(
            cacheKey,
            reports,
            TimeSpan.FromMinutes(_reportsListTtlInMinutes));

        return reports;
    }

    public async Task<string> GetReportUrl(string reportName)
    {
        var (id,report) = await _reportRepository.GetReportByName(reportName);
        var generatedDate = report.GeneratedAt ?? _timeProvider.GetUtcNow().UtcDateTime;
        var fileName = $"{generatedDate.Year}/{generatedDate.Month}/{reportName}";
        var filePath = await _minioService.GetFileLinkAsync(
            _minioOptions.BucketName, fileName);
        report.FilePath = filePath;
        await _reportRepository.UpdateReport(id,report);
        return filePath;
    }

    public async Task<GeneratedReport> GenerateWeeklyReport(DateTime startDate, DateTime endDate)
    {
        var logs = await _activityLogRepository.GetLogsAsync(
            DateOnly.FromDateTime(startDate),
            DateOnly.FromDateTime(endDate),
            Array.Empty<string>());

        var timestamp = _timeProvider.GetUtcNow().UtcDateTime;
        var fileName = $"weekly_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}_{timestamp:yyyyMMddHHmmss}.csv";
        var objectName = $"weekly/{timestamp.Year}/{timestamp.Month}/{fileName}";

        var sb = new StringBuilder();
        sb.AppendLine("EventType;EventDate;Metadata");
        foreach (var log in logs)
            sb.AppendLine($"{log.EventType};{log.EventDate};{JsonSerializer.Serialize(log, log.GetType())}");

        const string weeklyBucket = "library-reports";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
        await _minioService.UploadFileAsync(weeklyBucket, objectName, stream, "text/csv");
        var downloadUrl = await _minioService.GetFileLinkAsync(weeklyBucket, objectName);

        return new GeneratedReport
        {
            FileName = fileName,
            FilePath = objectName,
            DownloadUrl = downloadUrl,
            GeneratedAt = timestamp,
        };
    }
}