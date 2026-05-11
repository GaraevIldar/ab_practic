using Microsoft.Extensions.Options;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Configuration;
using PracticalWork.Library.Models;
using Quartz;

namespace PracticalWork.Library.Web.Jobs;

[DisallowConcurrentExecution]
public sealed class ReturnRemindersJob : IJob
{
    private readonly ILibraryRepository _libraryRepository;
    private readonly IEmailService _emailService;
    private readonly EmailTemplateSettings _templateSettings;
    private readonly ILogger<ReturnRemindersJob> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly TimeProvider _timeProvider;

    public ReturnRemindersJob(
        ILibraryRepository libraryRepository,
        IEmailService emailService,
        IOptions<EmailTemplateSettings> templateSettings,
        ILogger<ReturnRemindersJob> logger,
        IWebHostEnvironment env,
        TimeProvider timeProvider)
    {
        _libraryRepository = libraryRepository;
        _emailService = emailService;
        _templateSettings = templateSettings.Value;
        _logger = logger;
        _env = env;
        _timeProvider = timeProvider;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("ReturnRemindersJob started at {Time}", _timeProvider.GetUtcNow());

        var daysAhead = _templateSettings.ReturnReminder.DaysBeforeDueDate;
        var borrows = await _libraryRepository.GetBorrowsDueSoon(daysAhead);

        _logger.LogInformation("Found {Count} borrows due in {Days} days", borrows.Count, daysAhead);

        var template = await LoadTemplate("return_reminder.html");
        int sent = 0, skipped = 0;

        foreach (var borrow in borrows)
        {
            if (string.IsNullOrWhiteSpace(borrow.ReaderEmail))
            {
                _logger.LogDebug("Borrow {BorrowId}: reader has no email, skipping", borrow.BorrowId);
                skipped++;
                continue;
            }

            var alreadySent = await _libraryRepository.HasNotificationBeenSent(
                borrow.BorrowId, "ReturnReminder", TimeSpan.FromHours(24));

            if (alreadySent)
            {
                _logger.LogDebug("Borrow {BorrowId}: reminder already sent within 24h, skipping", borrow.BorrowId);
                skipped++;
                continue;
            }

            var html = BuildReminderHtml(template, borrow, daysAhead);
            var subject = _templateSettings.ReturnReminder.SubjectTemplate
                .Replace("{BookTitle}", borrow.BookTitle);

            var result = await _emailService.SendAsync(new EmailMessage
            {
                To = borrow.ReaderEmail,
                Subject = subject,
                HtmlBody = html,
                PlainTextBody = $"Уважаемый(ая) {borrow.ReaderFullName}, книга \"{borrow.BookTitle}\" должна быть возвращена {borrow.DueDate:dd.MM.yyyy}.",
            });

            if (result.IsSuccess)
            {
                await _libraryRepository.RecordNotification(borrow.BorrowId, "ReturnReminder");
                sent++;
            }
            else
            {
                _logger.LogWarning("Failed to send reminder for borrow {BorrowId}: {Error}",
                    borrow.BorrowId, result.ErrorMessage);
            }
        }

        _logger.LogInformation("ReturnRemindersJob finished: sent={Sent}, skipped={Skipped}", sent, skipped);
    }

    private string BuildReminderHtml(string template, BorrowWithDetails borrow, int daysLeft) =>
        template
            .Replace("{{LibraryName}}", _templateSettings.ReturnReminder.LibraryName)
            .Replace("{{ReaderName}}", borrow.ReaderFullName)
            .Replace("{{DaysLeft}}", daysLeft.ToString())
            .Replace("{{BookTitle}}", borrow.BookTitle)
            .Replace("{{BookAuthors}}", string.Join(", ", borrow.BookAuthors))
            .Replace("{{DueDate}}", borrow.DueDate.ToString("dd.MM.yyyy"))
            .Replace("{{LibraryAddress}}", _templateSettings.ReturnReminder.LibraryAddress)
            .Replace("{{LibraryPhone}}", _templateSettings.ReturnReminder.LibraryPhone)
            .Replace("{{WorkingHours}}", _templateSettings.ReturnReminder.WorkingHours);

    private async Task<string> LoadTemplate(string fileName)
    {
        var path = Path.Combine(_env.ContentRootPath, "resources", fileName);
        return await File.ReadAllTextAsync(path);
    }
}
