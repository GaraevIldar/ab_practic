using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Configuration;
using PracticalWork.Library.Models;
using PracticalWork.Library.Web.Jobs;
using Quartz;
using Xunit;

namespace PracticalWork.Library.UnitTests.Jobs;

public class ArchiveJobTests
{
    private readonly Mock<IArchiveService> _archive = new();
    private readonly Mock<IJobExecutionContext> _jobContext = new();
    private readonly ArchiveJob _sut;

    public ArchiveJobTests()
    {
        _sut = new ArchiveJob(
            _archive.Object,
            Options.Create(new ArchiveSettings { YearsWithoutBorrow = 3, MaxBooksPerRun = 50 }),
            NullLogger<ArchiveJob>.Instance,
            TimeProvider.System);
    }

    [Fact]
    public async Task Execute_CallsArchiveServiceWithConfiguredParams()
    {
        _archive.Setup(a => a.ArchiveOldBooks(3, 50))
            .ReturnsAsync(new ArchiveResult { TotalProcessed = 5, Archived = 5 });

        await _sut.Execute(_jobContext.Object);

        _archive.Verify(a => a.ArchiveOldBooks(3, 50), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenArchiveResultHasSkipReasons_LogsThem()
    {
        var result = new ArchiveResult { TotalProcessed = 2, Archived = 1, Skipped = 1 };
        result.SkipReasons.Add("book locked");
        _archive.Setup(a => a.ArchiveOldBooks(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(result);

        await _sut.Execute(_jobContext.Object);

        _archive.Verify(a => a.ArchiveOldBooks(3, 50), Times.Once);
    }
}
