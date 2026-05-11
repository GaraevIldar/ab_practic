using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PracticalWork.Library.Abstractions.MessageBroker;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Models;
using PracticalWork.Library.Services;
using PracticalWork.Library.UnitTests.TestHelpers;
using Xunit;

namespace PracticalWork.Library.UnitTests.Services;

public class ArchiveServiceTests
{
    private readonly Mock<IBookRepository> _bookRepo = new();
    private readonly Mock<IRabbitPublisher> _publisher = new();
    private readonly ArchiveService _sut;

    public ArchiveServiceTests()
    {
        _sut = new ArchiveService(
            _bookRepo.Object,
            _publisher.Object,
            NullLogger<ArchiveService>.Instance,
            TestRabbitConfig.Create(),
            TimeProvider.System);
    }

    [Fact]
    public async Task ArchiveOldBooks_NoBooks_ReturnsEmptyResult()
    {
        _bookRepo.Setup(r => r.GetBooksForArchive(3, 100)).ReturnsAsync(Array.Empty<Book>());

        var result = await _sut.ArchiveOldBooks(3, 100);

        result.TotalProcessed.Should().Be(0);
        result.Archived.Should().Be(0);
        result.Skipped.Should().Be(0);
        _publisher.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<It.IsAnyType>()), Times.Never);
    }

    [Fact]
    public async Task ArchiveOldBooks_ArchivesEachBook_PublishesEventPerBook()
    {
        var books = new List<Book>
        {
            new() { Id = Guid.NewGuid(), Title = "Book1" },
            new() { Id = Guid.NewGuid(), Title = "Book2" }
        };
        _bookRepo.Setup(r => r.GetBooksForArchive(3, 100)).ReturnsAsync(books);

        var result = await _sut.ArchiveOldBooks(3, 100);

        result.TotalProcessed.Should().Be(2);
        result.Archived.Should().Be(2);
        result.Skipped.Should().Be(0);
        _bookRepo.Verify(r => r.MoveToArchive(It.IsAny<Guid>()), Times.Exactly(2));
        _publisher.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<It.IsAnyType>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ArchiveOldBooks_WhenMoveToArchiveFails_RecordsSkipReason()
    {
        var books = new List<Book>
        {
            new() { Id = Guid.NewGuid(), Title = "Bad" },
            new() { Id = Guid.NewGuid(), Title = "Good" }
        };
        _bookRepo.Setup(r => r.GetBooksForArchive(3, 100)).ReturnsAsync(books);
        _bookRepo.Setup(r => r.MoveToArchive(books[0].Id)).ThrowsAsync(new InvalidOperationException("locked"));

        var result = await _sut.ArchiveOldBooks(3, 100);

        result.TotalProcessed.Should().Be(2);
        result.Archived.Should().Be(1);
        result.Skipped.Should().Be(1);
        result.SkipReasons.Should().ContainSingle().Which.Should().Contain("locked");
    }
}
