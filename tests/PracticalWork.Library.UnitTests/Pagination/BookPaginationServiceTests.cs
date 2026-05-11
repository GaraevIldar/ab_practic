using FluentAssertions;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Services;
using Xunit;

namespace PracticalWork.Library.UnitTests.Pagination;

public class BookPaginationServiceTests
{
    private readonly BookPaginationService _sut = new();

    private static BookListResponse MakeSource(int totalItems)
    {
        var items = Enumerable.Range(1, totalItems)
            .Select(i => new BookItemResponse { Id = Guid.NewGuid(), Title = $"Book {i}" })
            .ToList();
        return new BookListResponse { Books = items, TotalCount = totalItems };
    }

    [Fact]
    public void PaginationBooks_NullSource_Throws()
    {
        var act = () => _sut.PaginationBooks(null!, 1, 10);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PaginationBooks_FirstPage_ReturnsTopItems()
    {
        var source = MakeSource(25);

        var result = _sut.PaginationBooks(source, 1, 10);

        result.Books.Should().HaveCount(10);
        result.Books[0].Title.Should().Be("Book 1");
        result.TotalCount.Should().Be(25);
    }

    [Fact]
    public void PaginationBooks_SecondPage_SkipsCorrectly()
    {
        var source = MakeSource(25);

        var result = _sut.PaginationBooks(source, 2, 10);

        result.Books.Should().HaveCount(10);
        result.Books[0].Title.Should().Be("Book 11");
    }

    [Fact]
    public void PaginationBooks_LastPartialPage_ReturnsRemainder()
    {
        var source = MakeSource(25);

        var result = _sut.PaginationBooks(source, 3, 10);

        result.Books.Should().HaveCount(5);
        result.Books[0].Title.Should().Be("Book 21");
    }

    [Fact]
    public void PaginationBooks_PageNumberLessThanOne_DefaultsToOne()
    {
        var source = MakeSource(10);

        var result = _sut.PaginationBooks(source, 0, 5);

        result.Books.Should().HaveCount(5);
        result.Books[0].Title.Should().Be("Book 1");
    }

    [Fact]
    public void PaginationBooks_PageSizeLessThanOne_DefaultsToTen()
    {
        var source = MakeSource(25);

        var result = _sut.PaginationBooks(source, 1, 0);

        result.Books.Should().HaveCount(10);
    }

    [Fact]
    public void PaginationBooks_EmptySource_ReturnsEmpty()
    {
        var source = MakeSource(0);

        var result = _sut.PaginationBooks(source, 1, 10);

        result.Books.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public void PaginationBooks_PageBeyondData_ReturnsEmpty()
    {
        var source = MakeSource(5);

        var result = _sut.PaginationBooks(source, 10, 10);

        result.Books.Should().BeEmpty();
        result.TotalCount.Should().Be(5);
    }
}
