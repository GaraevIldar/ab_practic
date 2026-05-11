using Microsoft.Extensions.Options;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Configuration;
using PracticalWork.Library.Models;
using Quartz;

namespace PracticalWork.Library.Web.Jobs;

[DisallowConcurrentExecution]
public sealed class WeeklyReportJob : IJob
{
    private readonly IReportService _reportService;
    private readonly ILibraryRepository _libraryRepository;
    private readonly IEmailService _emailService;
    private readonly EmailSettings _emailSettings;
    private readonly EmailTemplateSettings _templateSettings;
    private readonly ILogger<WeeklyReportJob> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly TimeProvider _timeProvider;

    public WeeklyReportJob(
        IReportService reportService,
        ILibraryRepository libraryRepository,
        IEmailService emailService,
        IOptions<EmailSettings> emailSettings,
        IOptions<EmailTemplateSettings> templateSettings,
        ILogger<WeeklyReportJob> logger,
        IWebHostEnvironment env,
        TimeProvider timeProvider)
    {
        _reportService = reportService;
        _libraryRepository = libraryRepository;
        _emailService = emailService;
        _emailSettings = emailSettings.Value;
        _templateSettings = templateSettings.Value;
        _logger = logger;
        _env = env;
        _timeProvider = timeProvider;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var endDate = now.Date;
        var startDate = endDate.AddDays(-7);

        _logger.LogInformation("WeeklyReportJob started: {From} — {To}", startDate, endDate);

        GeneratedReport report;
        WeeklyStats stats;
        try
        {
            report = await _reportService.GenerateWeeklyReport(startDate, endDate);
            stats = await _libraryRepository.GetWeeklyStats(startDate, endDate);
            _logger.LogInformation("Weekly report generated: {FileName}", report.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate weekly report");
            return;
        }

        var adminEmails = _emailSettings.AdminEmails;
        if (adminEmails.Count == 0)
        {
            _logger.LogWarning("No admin emails configured, skipping weekly report notification");
            return;
        }

        var template = await LoadTemplate("weekly_report.html");
        var subject = _templateSettings.WeeklyReport.SubjectTemplate
            .Replace("{StartDate}", startDate.ToString("dd.MM.yyyy"))
            .Replace("{EndDate}", endDate.ToString("dd.MM.yyyy"));

        foreach (var adminEmail in adminEmails)
        {
            var html = template
                .Replace("{{PeriodFrom}}", startDate.ToString("dd.MM.yyyy"))
                .Replace("{{PeriodTo}}", endDate.ToString("dd.MM.yyyy"))
                .Replace("{{NewBooks}}", stats.NewBooks.ToString())
                .Replace("{{NewReaders}}", stats.NewReaders.ToString())
                .Replace("{{BooksBorrowed}}", stats.BooksBorrowed.ToString())
                .Replace("{{BooksReturned}}", stats.BooksReturned.ToString())
                .Replace("{{OverdueBorrows}}", stats.OverdueBorrows.ToString())
                .Replace("{{ReportUrl}}", report.DownloadUrl);

            var result = await _emailService.SendAsync(new EmailMessage
            {
                To = adminEmail,
                Subject = subject,
                HtmlBody = html,
                PlainTextBody = $"Еженедельный отчет за {startDate:dd.MM.yyyy}—{endDate:dd.MM.yyyy}. Ссылка: {report.DownloadUrl}",
            });

            if (!result.IsSuccess)
                _logger.LogWarning("Failed to send weekly report to {Email}: {Error}", adminEmail, result.ErrorMessage);
        }

        _logger.LogInformation("WeeklyReportJob finished");
    }

    private async Task<string> LoadTemplate(string fileName)
    {
        var path = Path.Combine(_env.ContentRootPath, "resources", fileName);
        return await File.ReadAllTextAsync(path);
    }
}
