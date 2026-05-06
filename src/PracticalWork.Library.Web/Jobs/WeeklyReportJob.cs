using Microsoft.Extensions.Options;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Configuration;
using PracticalWork.Library.Models;
using Quartz;

namespace PracticalWork.Library.Web.Jobs;

[DisallowConcurrentExecution]
public sealed class WeeklyReportJob : IJob
{
    private readonly IReportService _reportService;
    private readonly IEmailService _emailService;
    private readonly EmailSettings _emailSettings;
    private readonly EmailTemplateSettings _templateSettings;
    private readonly ILogger<WeeklyReportJob> _logger;
    private readonly IWebHostEnvironment _env;

    public WeeklyReportJob(
        IReportService reportService,
        IEmailService emailService,
        IOptions<EmailSettings> emailSettings,
        IOptions<EmailTemplateSettings> templateSettings,
        ILogger<WeeklyReportJob> logger,
        IWebHostEnvironment env)
    {
        _reportService = reportService;
        _emailService = emailService;
        _emailSettings = emailSettings.Value;
        _templateSettings = templateSettings.Value;
        _logger = logger;
        _env = env;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var endDate = DateTime.UtcNow.Date;
        var startDate = endDate.AddDays(-7);

        _logger.LogInformation("WeeklyReportJob started: {From} — {To}", startDate, endDate);

        GeneratedReport report;
        try
        {
            report = await _reportService.GenerateWeeklyReport(startDate, endDate);
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
                .Replace("{{NewBooks}}", "—")
                .Replace("{{NewReaders}}", "—")
                .Replace("{{BooksBorrowed}}", "—")
                .Replace("{{BooksReturned}}", "—")
                .Replace("{{OverdueBorrows}}", "—")
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
