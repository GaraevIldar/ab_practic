using FluentAssertions;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Models;
using Xunit;

namespace PracticalWork.Library.UnitTests.Domain;

public class ReportTests
{
    [Fact]
    public void NewReport_HasInProgressStatusByDefault()
    {
        var report = new Models.Report();

        report.Status.Should().Be(ReportStatus.InProgress);
    }

    [Fact]
    public void MakeGenerated_SetsStatusGeneratedNameAndGeneratedAt()
    {
        var report = new Models.Report();
        const string fileName = "report_2026_05.csv";
        var before = DateTime.UtcNow;

        report.MakeGenerated(fileName);

        report.Status.Should().Be(ReportStatus.Generated);
        report.Name.Should().Be(fileName);
        report.GeneratedAt.Should().NotBeNull();
        report.GeneratedAt!.Value.Should().BeOnOrAfter(before);
    }
}
