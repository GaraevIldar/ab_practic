using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Configuration;
using PracticalWork.Library.Models;
using PracticalWork.Library.UnitTests.TestHelpers;
using PracticalWork.Library.Web.Jobs;
using Quartz;
using Xunit;

namespace PracticalWork.Library.UnitTests.Jobs;

public class WeeklyReportJobTests : IDisposable
{
    private readonly Mock<IReportService> _reportService = new();
    private readonly Mock<ILibraryRepository> _libraryRepo = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<IWebHostEnvironment> _env = new();
    private readonly Mock<IJobExecutionContext> _jobContext = new();
    private readonly TempContentRoot _content = new();

    private WeeklyReportJob CreateSut(EmailSettings emailSettings)
    {
        _content.WriteResource("weekly_report.html",
            "Period {{PeriodFrom}}-{{PeriodTo}}: " +
            "NB={{NewBooks}} NR={{NewReaders}} BB={{BooksBorrowed}} " +
            "BR={{BooksReturned}} OB={{OverdueBorrows}} URL={{ReportUrl}}");
        _env.SetupGet(e => e.ContentRootPath).Returns(_content.Path);

        var templates = new EmailTemplateSettings
        {
            WeeklyReport = new WeeklyReportTemplate
            {
                SubjectTemplate = "Отчёт {StartDate} - {EndDate}"
            }
        };

        return new WeeklyReportJob(
            _reportService.Object,
            _libraryRepo.Object,
            _emailService.Object,
            Options.Create(emailSettings),
            Options.Create(templates),
            NullLogger<WeeklyReportJob>.Instance,
            _env.Object,
            TimeProvider.System);
    }

    [Fact]
    public async Task Execute_WhenNoAdminEmails_SkipsSending()
    {
        var sut = CreateSut(new EmailSettings { AdminEmails = new List<string>() });
        _reportService.Setup(r => r.GenerateWeeklyReport(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new GeneratedReport { FileName = "f.csv", DownloadUrl = "url" });
        _libraryRepo.Setup(r => r.GetWeeklyStats(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new WeeklyStats());

        await sut.Execute(_jobContext.Object);

        _emailService.Verify(e => e.SendAsync(It.IsAny<EmailMessage>()), Times.Never);
    }

    [Fact]
    public async Task Execute_WhenReportGenerationFails_DoesNotSend()
    {
        var sut = CreateSut(new EmailSettings { AdminEmails = new List<string> { "a@a.com" } });
        _reportService.Setup(r => r.GenerateWeeklyReport(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ThrowsAsync(new InvalidOperationException("storage down"));

        await sut.Execute(_jobContext.Object);

        _emailService.Verify(e => e.SendAsync(It.IsAny<EmailMessage>()), Times.Never);
    }

    [Fact]
    public async Task Execute_FillsTemplateWithStatsAndSendsToAdmin()
    {
        var sut = CreateSut(new EmailSettings { AdminEmails = new List<string> { "admin@example.com" } });
        var report = new GeneratedReport { FileName = "weekly.csv", DownloadUrl = "https://x/url" };
        var stats = new WeeklyStats
        {
            NewBooks = 5,
            NewReaders = 3,
            BooksBorrowed = 8,
            BooksReturned = 6,
            OverdueBorrows = 1
        };
        _reportService.Setup(r => r.GenerateWeeklyReport(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(report);
        _libraryRepo.Setup(r => r.GetWeeklyStats(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(stats);
        _emailService.Setup(e => e.SendAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync(EmailSendResult.Success());

        await sut.Execute(_jobContext.Object);

        _emailService.Verify(e => e.SendAsync(It.Is<EmailMessage>(m =>
            m.To == "admin@example.com"
            && m.HtmlBody.Contains("NB=5")
            && m.HtmlBody.Contains("NR=3")
            && m.HtmlBody.Contains("BB=8")
            && m.HtmlBody.Contains("BR=6")
            && m.HtmlBody.Contains("OB=1")
            && m.HtmlBody.Contains("URL=https://x/url"))), Times.Once);
    }

    [Fact]
    public async Task Execute_SendsToEveryAdmin()
    {
        var sut = CreateSut(new EmailSettings
        {
            AdminEmails = new List<string> { "a@x.com", "b@x.com" }
        });
        _reportService.Setup(r => r.GenerateWeeklyReport(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new GeneratedReport { FileName = "f.csv", DownloadUrl = "url" });
        _libraryRepo.Setup(r => r.GetWeeklyStats(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new WeeklyStats());
        _emailService.Setup(e => e.SendAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync(EmailSendResult.Success());

        await sut.Execute(_jobContext.Object);

        _emailService.Verify(e => e.SendAsync(It.IsAny<EmailMessage>()), Times.Exactly(2));
    }

    public void Dispose() => _content.Dispose();
}
