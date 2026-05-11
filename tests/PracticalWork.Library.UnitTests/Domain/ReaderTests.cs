using FluentAssertions;
using PracticalWork.Library.Models;
using Xunit;

namespace PracticalWork.Library.UnitTests.Domain;

public class ReaderTests
{
    [Fact]
    public void IsExpired_WhenExpiryDateInPast_ReturnsTrue()
    {
        var reader = new Reader
        {
            ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))
        };

        reader.IsExpired().Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenExpiryDateInFuture_ReturnsFalse()
    {
        var reader = new Reader
        {
            ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30))
        };

        reader.IsExpired().Should().BeFalse();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var reader = new Reader { IsActive = true };

        reader.Deactivate();

        reader.IsActive.Should().BeFalse();
    }

    [Fact]
    public void ExtendExpiry_WithNewerDate_UpdatesExpiry()
    {
        var current = DateOnly.FromDateTime(DateTime.UtcNow);
        var newDate = current.AddYears(1);
        var reader = new Reader { ExpiryDate = current };

        reader.ExtendExpiry(newDate);

        reader.ExpiryDate.Should().Be(newDate);
    }

    [Fact]
    public void ExtendExpiry_WithSameDate_Throws()
    {
        var current = DateOnly.FromDateTime(DateTime.UtcNow);
        var reader = new Reader { ExpiryDate = current };

        var act = () => reader.ExtendExpiry(current);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ExtendExpiry_WithEarlierDate_Throws()
    {
        var current = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));
        var earlier = current.AddDays(-5);
        var reader = new Reader { ExpiryDate = current };

        var act = () => reader.ExtendExpiry(earlier);

        act.Should().Throw<InvalidOperationException>();
    }
}
