using FluentAssertions;
using PracticalWork.Library.Models;
using PracticalWork.Library.Services;
using Xunit;

namespace PracticalWork.Library.UnitTests.Pagination;

public class ActivityLogPaginationServiceTests
{
    private readonly ActivityLogPaginationService _sut = new();

    private static IReadOnlyList<ActivityLog> MakeLogs(int count) =>
        Enumerable.Range(1, count)
            .Select(i => new ActivityLog { EventType = $"type{i}", EventDate = DateTime.UtcNow })
            .ToList();

    [Fact]
    public void PaginationLogs_NullSource_Throws()
    {
        var act = () => _sut.PaginationLogs(null!, 1, 10);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PaginationLogs_FirstPage_ReturnsItems()
    {
        var logs = MakeLogs(20);

        var result = _sut.PaginationLogs(logs, 1, 5);

        result.Should().HaveCount(5);
        result[0].EventType.Should().Be("type1");
    }

    [Fact]
    public void PaginationLogs_NegativePage_DefaultsToOne()
    {
        var logs = MakeLogs(5);

        var result = _sut.PaginationLogs(logs, -1, 5);

        result.Should().HaveCount(5);
    }

    [Fact]
    public void PaginationLogs_PageSizeZero_DefaultsToTen()
    {
        var logs = MakeLogs(15);

        var result = _sut.PaginationLogs(logs, 1, 0);

        result.Should().HaveCount(10);
    }

    [Fact]
    public void PaginationLogs_EmptySource_ReturnsEmpty()
    {
        var result = _sut.PaginationLogs(Array.Empty<ActivityLog>(), 1, 10);

        result.Should().BeEmpty();
    }
}
