using FluentAssertions;
using Moq;
using PracticalWork.Library.Abstractions.MessageBroker;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Data.Minio;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Exceptions;
using PracticalWork.Library.Exceptions.Book;
using PracticalWork.Library.Exceptions.Library;
using PracticalWork.Library.Exceptions.Reader;
using PracticalWork.Library.Models;
using PracticalWork.Library.Services;
using PracticalWork.Library.UnitTests.TestHelpers;
using Xunit;

namespace PracticalWork.Library.UnitTests.Services;

public class LibraryServiceTests
{
    private readonly Mock<ILibraryRepository> _libraryRepo = new();
    private readonly Mock<IBookRepository> _bookRepo = new();
    private readonly Mock<IReaderRepository> _readerRepo = new();
    private readonly Mock<IMinioService> _minio = new();
    private readonly Mock<ICacheService> _cache = new();
    private readonly Mock<IBookPaginationService> _pagination = new();
    private readonly Mock<IRabbitPublisher> _publisher = new();
    private readonly LibraryService _sut;

    public LibraryServiceTests()
    {
        _sut = new LibraryService(
            _libraryRepo.Object,
            _bookRepo.Object,
            _readerRepo.Object,
            _minio.Object,
            _cache.Object,
            _pagination.Object,
            TestConfigurationBuilder.Build(),
            TestRabbitConfig.Create(),
            _publisher.Object);
    }

    [Fact]
    public async Task BorrowBook_WhenReaderNotFound_Throws()
    {
        var readerId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        _readerRepo.Setup(r => r.IsReaderExist(readerId)).ReturnsAsync(false);

        var act = () => _sut.BorrowBook(bookId, readerId);

        await act.Should().ThrowAsync<ReaderNotFoundException>();
    }

    [Fact]
    public async Task BorrowBook_WhenBookNotFound_Throws()
    {
        var readerId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        _readerRepo.Setup(r => r.IsReaderExist(readerId)).ReturnsAsync(true);
        _bookRepo.Setup(r => r.IsBookExist(bookId)).ReturnsAsync(false);

        var act = () => _sut.BorrowBook(bookId, readerId);

        await act.Should().ThrowAsync<BookNotFoundException>();
    }

    [Fact]
    public async Task BorrowBook_WhenBookNotAvailable_Throws()
    {
        var readerId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        _readerRepo.Setup(r => r.IsReaderExist(readerId)).ReturnsAsync(true);
        _bookRepo.Setup(r => r.IsBookExist(bookId)).ReturnsAsync(true);
        _bookRepo.Setup(r => r.GetBookById(bookId))
            .ReturnsAsync(new Book { Id = bookId, Status = BookStatus.Borrow });

        var act = () => _sut.BorrowBook(bookId, readerId);

        await act.Should().ThrowAsync<BookNotAvailableException>();
    }

    [Fact]
    public async Task BorrowBook_WhenValid_PublishesEventAndReturnsResponse()
    {
        var readerId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var borrow = new Borrow
        {
            Id = Guid.NewGuid(),
            BookId = bookId,
            ReaderId = readerId,
            BorrowDate = DateOnly.FromDateTime(DateTime.UtcNow),
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14))
        };
        _readerRepo.Setup(r => r.IsReaderExist(readerId)).ReturnsAsync(true);
        _bookRepo.Setup(r => r.IsBookExist(bookId)).ReturnsAsync(true);
        _bookRepo.Setup(r => r.GetBookById(bookId))
            .ReturnsAsync(new Book { Id = bookId, Status = BookStatus.Available, Title = "T" });
        _libraryRepo.Setup(r => r.BorrowBook(bookId, readerId)).ReturnsAsync(borrow);
        _readerRepo.Setup(r => r.GetReaderFullNameById(readerId)).ReturnsAsync("Иванов");

        var result = await _sut.BorrowBook(bookId, readerId);

        result.Id.Should().Be(borrow.Id);
        _publisher.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<It.IsAnyType>()), Times.Once);
    }

    [Fact]
    public async Task ReturnBook_WhenBorrowNotFound_Throws()
    {
        var bookId = Guid.NewGuid();
        _libraryRepo.Setup(r => r.GetBookBorrow(bookId)).ReturnsAsync((Borrow)null!);

        var act = () => _sut.ReturnBook(bookId);

        await act.Should().ThrowAsync<BookBorrowNotFoundException>();
    }

    [Fact]
    public async Task ReturnBook_WhenValid_PublishesEventAndReturnsResponse()
    {
        var bookId = Guid.NewGuid();
        var readerId = Guid.NewGuid();
        var returnId = Guid.NewGuid();
        var borrow = new Borrow { Id = Guid.NewGuid(), BookId = bookId, ReaderId = readerId };

        _libraryRepo.Setup(r => r.GetBookBorrow(bookId)).ReturnsAsync(borrow);
        _bookRepo.Setup(r => r.GetBookById(bookId)).ReturnsAsync(new Book { Id = bookId, Title = "T" });
        _readerRepo.Setup(r => r.GetReaderFullNameById(readerId)).ReturnsAsync("Петров");
        _libraryRepo.Setup(r => r.ReturnBook(bookId)).ReturnsAsync(returnId);

        var result = await _sut.ReturnBook(bookId);

        result.Id.Should().Be(returnId);
        _publisher.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<It.IsAnyType>()), Times.Once);
    }

    [Fact]
    public async Task GetBooksNoArchive_CacheHit_ReturnsFromCache()
    {
        var cached = new BookListResponse { Books = new List<BookItemResponse>(), TotalCount = 0 };
        _cache.Setup(c => c.GetCurrentCacheVersion(It.IsAny<string>())).ReturnsAsync(1);
        _cache.Setup(c => c.GenerateCacheKey(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<object>())).Returns("k");
        _cache.Setup(c => c.GetAsync<BookListResponse>("k")).ReturnsAsync(cached);

        var result = await _sut.GetBooksNoArchive(1, 10, null, null);

        result.Should().BeSameAs(cached);
        _bookRepo.Verify(r => r.GetBooksNoArchive(It.IsAny<PracticalWork.Library.Contracts.v1.Enums.BookCategory?>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetBooksNoArchive_CacheMiss_FetchesAndPaginates()
    {
        var raw = new BookListResponse { Books = new List<BookItemResponse>(), TotalCount = 0 };
        var paged = new BookListResponse { Books = new List<BookItemResponse>(), TotalCount = 0 };

        _cache.Setup(c => c.GetCurrentCacheVersion(It.IsAny<string>())).ReturnsAsync(1);
        _cache.Setup(c => c.GenerateCacheKey(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<object>())).Returns("k");
        _cache.Setup(c => c.GetAsync<BookListResponse>("k")).ReturnsAsync((BookListResponse)null!);
        _bookRepo.Setup(r => r.GetBooksNoArchive(
                It.IsAny<PracticalWork.Library.Contracts.v1.Enums.BookCategory?>(), It.IsAny<string>()))
            .ReturnsAsync(raw);
        _pagination.Setup(p => p.PaginationBooks(raw, 1, 10)).Returns(paged);

        var result = await _sut.GetBooksNoArchive(1, 10, null, null);

        result.Should().BeSameAs(paged);
        _cache.Verify(c => c.SetAsync("k", raw, It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Fact]
    public async Task GetBookDetailsAsync_ByGuid_ReturnsResponseWithCoverUrl()
    {
        var id = Guid.NewGuid();
        var book = new Book
        {
            Id = id,
            Title = "T",
            Authors = new[] { "A" },
            Description = "D",
            Year = 2020,
            CoverImagePath = "covers/x.jpg",
            Status = BookStatus.Available
        };
        _bookRepo.Setup(r => r.GetBookById(id)).ReturnsAsync(book);
        _minio.SetupGet(m => m.Endpoint).Returns("minio:9000");

        var result = await _sut.GetBookDetailsAsync(id.ToString());

        result.Id.Should().Be(id);
        result.CoverImagePath.Should().Contain("minio:9000").And.Contain("covers/x.jpg");
        result.IsArchived.Should().BeFalse();
    }

    [Fact]
    public async Task GetBookDetailsAsync_ByTitle_QueriesByTitle()
    {
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Война и мир",
            Authors = new[] { "Л. Толстой" },
            Description = "Роман",
            Year = 1869,
            CoverImagePath = string.Empty,
            Status = BookStatus.Archived
        };
        _bookRepo.Setup(r => r.GetBookByTitle("Война и мир")).ReturnsAsync(book);

        var result = await _sut.GetBookDetailsAsync("Война и мир");

        result.Title.Should().Be("Война и мир");
        result.CoverImagePath.Should().Be(string.Empty);
        result.IsArchived.Should().BeTrue();
    }
}
