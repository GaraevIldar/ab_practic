using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using PracticalWork.Library.Abstractions.MessageBroker;
using PracticalWork.Library.Abstractions.Minio;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v1.Pagination;
using PracticalWork.Library.Data.Minio;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Exceptions.Report;
using PracticalWork.Library.Models;
using PracticalWork.Library.Services;
using PracticalWork.Library.UnitTests.TestHelpers;
using Xunit;

namespace PracticalWork.Library.UnitTests.Services;

public class ReportServiceTests
{
    private readonly Mock<IActivityLogRepository> _activityRepo = new();
    private readonly Mock<IActivityLogPaginationService> _pagination = new();
    private readonly Mock<IReportRepository> _reportRepo = new();
    private readonly Mock<IRabbitPublisher> _publisher = new();
    private readonly Mock<ICacheService> _cache = new();
    private readonly Mock<IMinioService> _minio = new();
    private readonly Mock<IOptionsMonitor<MinioOptions>> _minioOptions = new();
    private readonly ReportService _sut;

    public ReportServiceTests()
    {
        _minioOptions.Setup(m => m.CurrentValue).Returns(new MinioOptions
        {
            Endpoint = "minio:9000",
            BucketName = "reports",
            ExpInSeconds = 86400
        });

        _sut = new ReportService(
            _activityRepo.Object,
            _pagination.Object,
            _reportRepo.Object,
            _publisher.Object,
            TestConfigurationBuilder.Build(),
            _cache.Object,
            _minio.Object,
            _minioOptions.Object,
            TimeProvider.System);
    }

    [Fact]
    public async Task SaveActivityLogs_PassesToRepository()
    {
        var log = new ActivityLog { EventType = "book.created" };

        await _sut.SaveActivityLogs(log);

        _activityRepo.Verify(r => r.AddLogAsync(log), Times.Once);
    }

    [Fact]
    public async Task GetActivityLogs_ReturnsPaginatedResults()
    {
        var request = new ActivityLogsPaginationRequest(1, 10);
        var logs = new List<ActivityLog> { new() { EventType = "book.created" } };
        _activityRepo.Setup(r => r.GetLogsPageAsync(request)).ReturnsAsync(logs);
        _pagination.Setup(p => p.PaginationLogs(logs, 1, 10)).Returns(logs);

        var result = await _sut.GetActivityLogs(request);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateReport_CreatesPublishesAndInvalidates()
    {
        var newId = Guid.NewGuid();
        _reportRepo.Setup(r => r.CreateReport(It.IsAny<Models.Report>())).ReturnsAsync(newId);

        var result = await _sut.CreateReport(null, null, Array.Empty<string>());

        result.Status.Should().Be(ReportStatus.InProgress);
        _publisher.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<It.IsAnyType>()), Times.Once);
        _cache.Verify(c => c.InvalidateCache(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GenerateReport_WhenReportNotFound_Throws()
    {
        var reportId = Guid.NewGuid();
        _reportRepo.Setup(r => r.GetReportById(reportId)).ReturnsAsync((Models.Report)null!);

        var act = () => _sut.GenerateReport(reportId, null, null, Array.Empty<string>());

        await act.Should().ThrowAsync<ReportNotFoundException>();
    }

    [Fact]
    public async Task GenerateReport_WhenSuccess_MarksReportGenerated()
    {
        var reportId = Guid.NewGuid();
        var report = new Models.Report();
        _reportRepo.Setup(r => r.GetReportById(reportId)).ReturnsAsync(report);
        _activityRepo.Setup(r => r.GetLogsAsync(null, null, It.IsAny<string[]>()))
            .ReturnsAsync(new List<ActivityLog>());

        await _sut.GenerateReport(reportId, null, null, Array.Empty<string>());

        report.Status.Should().Be(ReportStatus.Generated);
        _minio.Verify(m => m.UploadFileAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()), Times.Once);
        _reportRepo.Verify(r => r.UpdateReport(reportId, report), Times.Once);
    }

    [Fact]
    public async Task GenerateReport_WhenUploadFails_MarksReportErrorAndRethrows()
    {
        var reportId = Guid.NewGuid();
        var report = new Models.Report();
        _reportRepo.Setup(r => r.GetReportById(reportId)).ReturnsAsync(report);
        _activityRepo.Setup(r => r.GetLogsAsync(null, null, It.IsAny<string[]>()))
            .ReturnsAsync(new List<ActivityLog>());
        _minio.Setup(m => m.UploadFileAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("upload failed"));

        var act = () => _sut.GenerateReport(reportId, null, null, Array.Empty<string>());

        await act.Should().ThrowAsync<Exception>();
        report.Status.Should().Be(ReportStatus.Error);
    }

    [Fact]
    public async Task GetListReadyReports_CacheHit_DoesNotQueryRepo()
    {
        var cached = new List<Models.Report> { new() { Status = ReportStatus.Generated, Name = "x" } };
        _cache.Setup(c => c.GetCurrentCacheVersion(It.IsAny<string>())).ReturnsAsync(1);
        _cache.Setup(c => c.GenerateCacheKey(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<object?>())).Returns("k");
        _cache.Setup(c => c.GetAsync<IReadOnlyList<Models.Report>>("k")).ReturnsAsync(cached);

        var result = await _sut.GetListReadyReports();

        result.Should().BeSameAs(cached);
        _reportRepo.Verify(r => r.GetReadyReports(), Times.Never);
    }

    [Fact]
    public async Task GetListReadyReports_CacheMiss_QueriesAndCaches()
    {
        IReadOnlyList<Models.Report> fromRepo = new List<Models.Report> { new() { Status = ReportStatus.Generated, Name = "x" } };
        _cache.Setup(c => c.GetCurrentCacheVersion(It.IsAny<string>())).ReturnsAsync(1);
        _cache.Setup(c => c.GenerateCacheKey(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<object?>())).Returns("k");
        _cache.Setup(c => c.GetAsync<IReadOnlyList<Models.Report>>("k")).ReturnsAsync((IReadOnlyList<Models.Report>)null!);
        _reportRepo.Setup(r => r.GetReadyReports()).ReturnsAsync(fromRepo);

        var result = await _sut.GetListReadyReports();

        result.Should().BeSameAs(fromRepo);
        _cache.Verify(c => c.SetAsync<IReadOnlyList<Models.Report>>("k", fromRepo, It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Fact]
    public async Task GetReportUrl_ReturnsLinkAndUpdatesFilePath()
    {
        var id = Guid.NewGuid();
        var report = new Models.Report { Name = "r.csv", GeneratedAt = new DateTime(2026, 5, 19) };
        _reportRepo.Setup(r => r.GetReportByName("r.csv")).ReturnsAsync((id, report));
        _minio.Setup(m => m.GetFileLinkAsync("reports", It.IsAny<string>())).ReturnsAsync("https://minio/url");

        var result = await _sut.GetReportUrl("r.csv");

        result.Should().Be("https://minio/url");
        report.FilePath.Should().Be("https://minio/url");
        _reportRepo.Verify(r => r.UpdateReport(id, report), Times.Once);
    }

    [Fact]
    public async Task GenerateWeeklyReport_UploadsAndReturnsGeneratedReportWithUrl()
    {
        var start = new DateTime(2026, 5, 12);
        var end = new DateTime(2026, 5, 19);

        _activityRepo.Setup(r => r.GetLogsAsync(DateOnly.FromDateTime(start), DateOnly.FromDateTime(end), It.IsAny<string[]>()))
            .ReturnsAsync(new List<ActivityLog>());
        _minio.Setup(m => m.GetFileLinkAsync("library-reports", It.IsAny<string>()))
            .ReturnsAsync("https://minio/weekly");

        var result = await _sut.GenerateWeeklyReport(start, end);

        result.FileName.Should().StartWith("weekly_20260512_20260519_");
        result.DownloadUrl.Should().Be("https://minio/weekly");
        _minio.Verify(m => m.UploadFileAsync("library-reports", It.IsAny<string>(),
            It.IsAny<Stream>(), "text/csv"), Times.Once);
    }
}
