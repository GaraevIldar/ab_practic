using FluentAssertions;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Models;
using Xunit;

namespace PracticalWork.Library.UnitTests.Domain;

public class BookTests
{
    [Fact]
    public void CanBeArchived_WhenStatusIsAvailable_ReturnsTrue()
    {
        var book = new Book { Status = BookStatus.Available };

        book.CanBeArchived().Should().BeTrue();
    }

    [Fact]
    public void CanBeArchived_WhenStatusIsArchived_ReturnsTrue()
    {
        var book = new Book { Status = BookStatus.Archived };

        book.CanBeArchived().Should().BeTrue();
    }

    [Fact]
    public void CanBeArchived_WhenStatusIsBorrow_ReturnsFalse()
    {
        var book = new Book { Status = BookStatus.Borrow };

        book.CanBeArchived().Should().BeFalse();
    }

    [Fact]
    public void CanBeBorrowed_WhenAvailableAndNotArchived_ReturnsTrue()
    {
        var book = new Book { Status = BookStatus.Available, IsArchived = false };

        book.CanBeBorrowed().Should().BeTrue();
    }

    [Fact]
    public void CanBeBorrowed_WhenArchived_ReturnsFalse()
    {
        var book = new Book { Status = BookStatus.Available, IsArchived = true };

        book.CanBeBorrowed().Should().BeFalse();
    }

    [Fact]
    public void CanBeBorrowed_WhenStatusIsBorrow_ReturnsFalse()
    {
        var book = new Book { Status = BookStatus.Borrow, IsArchived = false };

        book.CanBeBorrowed().Should().BeFalse();
    }

    [Fact]
    public void Archive_WhenAvailable_ChangesStatusAndSetsFlag()
    {
        var book = new Book { Status = BookStatus.Available };

        book.Archive();

        book.IsArchived.Should().BeTrue();
        book.Status.Should().Be(BookStatus.Archived);
    }

    [Fact]
    public void Archive_WhenBorrowed_ThrowsInvalidOperationException()
    {
        var book = new Book { Status = BookStatus.Borrow };

        var act = () => book.Archive();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Книга не может быть заархивирована.");
    }

    [Fact]
    public void UpdateDetail_SetsDescriptionAndCoverPath()
    {
        var book = new Book();
        const string description = "Описание книги";
        const string cover = "covers/book.jpg";

        book.UpdateDetail(description, cover);

        book.Description.Should().Be(description);
        book.CoverImagePath.Should().Be(cover);
    }
}
