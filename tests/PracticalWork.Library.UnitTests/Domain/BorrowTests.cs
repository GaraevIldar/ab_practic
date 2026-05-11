using FluentAssertions;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Models;
using Xunit;

namespace PracticalWork.Library.UnitTests.Domain;

public class BorrowTests
{
    [Fact]
    public void IsOverdue_WhenIssuedAndPastDueDate_ReturnsTrue()
    {
        var borrow = new Borrow
        {
            Status = BookIssueStatus.Issued,
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))
        };

        borrow.IsOverdue().Should().BeTrue();
    }

    [Fact]
    public void IsOverdue_WhenIssuedAndDueDateInFuture_ReturnsFalse()
    {
        var borrow = new Borrow
        {
            Status = BookIssueStatus.Issued,
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5))
        };

        borrow.IsOverdue().Should().BeFalse();
    }

    [Fact]
    public void IsOverdue_WhenAlreadyReturned_ReturnsFalse()
    {
        var borrow = new Borrow
        {
            Status = BookIssueStatus.Returned,
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10))
        };

        borrow.IsOverdue().Should().BeFalse();
    }

    [Fact]
    public void MarkAsReturned_WhenIssuedOnTime_SetsReturnedStatusAndDate()
    {
        var borrow = new Borrow
        {
            Status = BookIssueStatus.Issued,
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5))
        };

        borrow.MarkAsReturned();

        borrow.Status.Should().Be(BookIssueStatus.Returned);
        borrow.ReturnDate.Should().Be(DateOnly.FromDateTime(DateTime.UtcNow));
    }

    [Fact]
    public void MarkAsReturned_WhenOverdueAtReturnTime_SetsOverdueStatus()
    {
        var borrow = new Borrow
        {
            Status = BookIssueStatus.Issued,
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))
        };

        borrow.MarkAsReturned();

        borrow.Status.Should().Be(BookIssueStatus.Overdue);
    }

    [Fact]
    public void MarkAsReturned_WhenAlreadyReturned_Throws()
    {
        var borrow = new Borrow { Status = BookIssueStatus.Returned };

        var act = () => borrow.MarkAsReturned();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CheckAndMarkOverdue_WhenOverdue_UpdatesStatus()
    {
        var borrow = new Borrow
        {
            Status = BookIssueStatus.Issued,
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2))
        };

        borrow.CheckAndMarkOverdue();

        borrow.Status.Should().Be(BookIssueStatus.Overdue);
    }

    [Fact]
    public void CheckAndMarkOverdue_WhenNotOverdue_KeepsStatus()
    {
        var borrow = new Borrow
        {
            Status = BookIssueStatus.Issued,
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2))
        };

        borrow.CheckAndMarkOverdue();

        borrow.Status.Should().Be(BookIssueStatus.Issued);
    }
}
