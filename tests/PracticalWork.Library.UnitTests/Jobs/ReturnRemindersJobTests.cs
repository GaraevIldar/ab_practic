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

public class ReturnRemindersJobTests : IDisposable
{
    private readonly Mock<ILibraryRepository> _libraryRepo = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<IWebHostEnvironment> _env = new();
    private readonly Mock<IJobExecutionContext> _jobContext = new();
    private readonly TempContentRoot _content = new();
    private readonly ReturnRemindersJob _sut;

    public ReturnRemindersJobTests()
    {
        _content.WriteResource("return_reminder.html",
            "Hello {{ReaderName}}, return {{BookTitle}} by {{DueDate}}.");
        _env.SetupGet(e => e.ContentRootPath).Returns(_content.Path);

        var templates = new EmailTemplateSettings
        {
            ReturnReminder = new ReturnReminderTemplate
            {
                SubjectTemplate = "Напоминание: \"{BookTitle}\"",
                DaysBeforeDueDate = 3,
                LibraryName = "Test Library"
            }
        };

        _sut = new ReturnRemindersJob(
            _libraryRepo.Object,
            _emailService.Object,
            Options.Create(templates),
            NullLogger<ReturnRemindersJob>.Instance,
            _env.Object,
            TimeProvider.System);
    }

    [Fact]
    public async Task Execute_WhenNoBorrows_SendsNothing()
    {
        _libraryRepo.Setup(r => r.GetBorrowsDueSoon(3))
            .ReturnsAsync(Array.Empty<BorrowWithDetails>());

        await _sut.Execute(_jobContext.Object);

        _emailService.Verify(e => e.SendAsync(It.IsAny<EmailMessage>()), Times.Never);
    }

    [Fact]
    public async Task Execute_SkipsBorrowsWithoutEmail()
    {
        var borrow = new BorrowWithDetails
        {
            BorrowId = Guid.NewGuid(),
            BookTitle = "T",
            BookAuthors = new[] { "A" },
            ReaderEmail = null,
            ReaderFullName = "R",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2))
        };
        _libraryRepo.Setup(r => r.GetBorrowsDueSoon(3))
            .ReturnsAsync(new[] { borrow });

        await _sut.Execute(_jobContext.Object);

        _emailService.Verify(e => e.SendAsync(It.IsAny<EmailMessage>()), Times.Never);
        _libraryRepo.Verify(r => r.RecordNotification(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Execute_SkipsBorrowsAlreadyNotified()
    {
        var borrow = new BorrowWithDetails
        {
            BorrowId = Guid.NewGuid(),
            BookTitle = "T",
            BookAuthors = new[] { "A" },
            ReaderEmail = "x@x.com",
            ReaderFullName = "R",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2))
        };
        _libraryRepo.Setup(r => r.GetBorrowsDueSoon(3)).ReturnsAsync(new[] { borrow });
        _libraryRepo.Setup(r => r.HasNotificationBeenSent(borrow.BorrowId, "ReturnReminder", It.IsAny<TimeSpan>()))
            .ReturnsAsync(true);

        await _sut.Execute(_jobContext.Object);

        _emailService.Verify(e => e.SendAsync(It.IsAny<EmailMessage>()), Times.Never);
    }

    [Fact]
    public async Task Execute_SendsEmailAndRecordsNotificationOnSuccess()
    {
        var borrow = new BorrowWithDetails
        {
            BorrowId = Guid.NewGuid(),
            BookTitle = "Война и мир",
            BookAuthors = new[] { "Л. Толстой" },
            ReaderEmail = "ivan@example.com",
            ReaderFullName = "Иванов",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3))
        };
        _libraryRepo.Setup(r => r.GetBorrowsDueSoon(3)).ReturnsAsync(new[] { borrow });
        _libraryRepo.Setup(r => r.HasNotificationBeenSent(borrow.BorrowId, "ReturnReminder", It.IsAny<TimeSpan>()))
            .ReturnsAsync(false);
        _emailService.Setup(e => e.SendAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync(EmailSendResult.Success());

        await _sut.Execute(_jobContext.Object);

        _emailService.Verify(e => e.SendAsync(It.Is<EmailMessage>(m =>
            m.To == "ivan@example.com"
            && m.Subject.Contains("Война и мир")
            && m.HtmlBody.Contains("Иванов")
            && m.HtmlBody.Contains("Война и мир"))), Times.Once);
        _libraryRepo.Verify(r => r.RecordNotification(borrow.BorrowId, "ReturnReminder"), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenEmailSendFails_DoesNotRecordNotification()
    {
        var borrow = new BorrowWithDetails
        {
            BorrowId = Guid.NewGuid(),
            BookTitle = "T",
            BookAuthors = new[] { "A" },
            ReaderEmail = "x@x.com",
            ReaderFullName = "R",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3))
        };
        _libraryRepo.Setup(r => r.GetBorrowsDueSoon(3)).ReturnsAsync(new[] { borrow });
        _libraryRepo.Setup(r => r.HasNotificationBeenSent(borrow.BorrowId, "ReturnReminder", It.IsAny<TimeSpan>()))
            .ReturnsAsync(false);
        _emailService.Setup(e => e.SendAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync(EmailSendResult.Failure("smtp down"));

        await _sut.Execute(_jobContext.Object);

        _libraryRepo.Verify(r => r.RecordNotification(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    public void Dispose() => _content.Dispose();
}
