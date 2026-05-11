using FluentAssertions;
using Moq;
using PracticalWork.Library.Abstractions.MessageBroker;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Data.Minio;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Exceptions.Book;
using PracticalWork.Library.Models;
using PracticalWork.Library.Services;
using PracticalWork.Library.UnitTests.TestHelpers;
using Xunit;
using BookCategory = PracticalWork.Library.Contracts.v1.Enums.BookCategory;

namespace PracticalWork.Library.UnitTests.Services;

public class BookServiceTests
{
    private readonly Mock<IBookRepository> _bookRepository = new();
    private readonly Mock<ICacheService> _cache = new();
    private readonly Mock<IMinioService> _minio = new();
    private readonly Mock<IBookPaginationService> _pagination = new();
    private readonly Mock<IRabbitPublisher> _publisher = new();
    private readonly BookService _sut;

    public BookServiceTests()
    {
        _sut = new BookService(
            _bookRepository.Object,
            _cache.Object,
            _minio.Object,
            _pagination.Object,
            TestConfigurationBuilder.Build(),
            TestRabbitConfig.Create(),
            _publisher.Object);
    }

    [Fact]
    public async Task CreateBook_SetsStatusAvailable_PublishesEventAndInvalidatesCache()
    {
        var book = new Book { Id = Guid.NewGuid(), Title = "T", Authors = new[] { "A" }, Year = 2020, Category = BookCategory.FictionBook };
        _bookRepository.Setup(r => r.CreateBook(book)).ReturnsAsync(book.Id);

        var id = await _sut.CreateBook(book);

        id.Should().Be(book.Id);
        book.Status.Should().Be(BookStatus.Available);
        _publisher.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<It.IsAnyType>()), Times.Once);
        _cache.Verify(c => c.InvalidateCache(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBook_WhenBookNotFound_Throws()
    {
        var id = Guid.NewGuid();
        _bookRepository.Setup(r => r.GetBookById(id)).ReturnsAsync((Book)null!);

        var act = () => _sut.UpdateBook(id, new Book());

        await act.Should().ThrowAsync<BookNotFoundException>();
    }

    [Fact]
    public async Task UpdateBook_WhenBookArchived_Throws()
    {
        var id = Guid.NewGuid();
        _bookRepository.Setup(r => r.GetBookById(id))
            .ReturnsAsync(new Book { Id = id, Status = BookStatus.Archived });

        var act = () => _sut.UpdateBook(id, new Book());

        await act.Should().ThrowAsync<BookArchivedException>();
    }

    [Fact]
    public async Task UpdateBook_WhenAvailable_UpdatesAndInvalidatesCache()
    {
        var id = Guid.NewGuid();
        _bookRepository.Setup(r => r.GetBookById(id))
            .ReturnsAsync(new Book { Id = id, Status = BookStatus.Available });

        await _sut.UpdateBook(id, new Book());

        _bookRepository.Verify(r => r.UpdateBook(id, It.IsAny<Book>()), Times.Once);
        _cache.Verify(c => c.InvalidateCache(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task MoveToArchive_WhenBookNotFound_Throws()
    {
        var id = Guid.NewGuid();
        _bookRepository.Setup(r => r.GetBookById(id)).ReturnsAsync((Book)null!);

        var act = () => _sut.MoveToArchive(id);

        await act.Should().ThrowAsync<BookNotFoundException>();
    }

    [Fact]
    public async Task MoveToArchive_WhenAlreadyArchived_Throws()
    {
        var id = Guid.NewGuid();
        _bookRepository.Setup(r => r.GetBookById(id))
            .ReturnsAsync(new Book { Id = id, Status = BookStatus.Archived });

        var act = () => _sut.MoveToArchive(id);

        await act.Should().ThrowAsync<BookAlreadyArchivedException>();
    }

    [Fact]
    public async Task MoveToArchive_WhenBorrowed_Throws()
    {
        var id = Guid.NewGuid();
        _bookRepository.Setup(r => r.GetBookById(id))
            .ReturnsAsync(new Book { Id = id, Status = BookStatus.Borrow });

        var act = () => _sut.MoveToArchive(id);

        await act.Should().ThrowAsync<BookBorrowedException>();
    }

    [Fact]
    public async Task MoveToArchive_WhenAvailable_MovesAndPublishesAndInvalidates()
    {
        var id = Guid.NewGuid();
        _bookRepository.Setup(r => r.GetBookById(id))
            .ReturnsAsync(new Book { Id = id, Status = BookStatus.Available, Title = "T" });

        await _sut.MoveToArchive(id);

        _bookRepository.Verify(r => r.MoveToArchive(id), Times.Once);
        _publisher.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<It.IsAnyType>()), Times.Once);
        _cache.Verify(c => c.InvalidateCache(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetBooks_WhenCacheHit_ReturnsCachedAndSkipsRepository()
    {
        var cached = new BookListResponse { Books = new List<BookItemResponse>(), TotalCount = 0 };
        _cache.Setup(c => c.GetCurrentCacheVersion(It.IsAny<string>())).ReturnsAsync(1);
        _cache.Setup(c => c.GenerateCacheKey(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<object>())).Returns("key");
        _cache.Setup(c => c.GetAsync<BookListResponse>("key")).ReturnsAsync(cached);

        var result = await _sut.GetBooks(1, 10, null, null, null);

        result.Should().BeSameAs(cached);
        _bookRepository.Verify(r => r.GetFilterBooks(It.IsAny<PracticalWork.Library.Contracts.v1.Enums.BookStatus?>(),
            It.IsAny<BookCategory?>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetBooks_WhenCacheMiss_FetchesPaginatesAndCaches()
    {
        var fromRepo = new BookListResponse { Books = new List<BookItemResponse>(), TotalCount = 0 };
        var paginated = new BookListResponse { Books = new List<BookItemResponse>(), TotalCount = 0 };

        _cache.Setup(c => c.GetCurrentCacheVersion(It.IsAny<string>())).ReturnsAsync(1);
        _cache.Setup(c => c.GenerateCacheKey(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<object>())).Returns("key");
        _cache.Setup(c => c.GetAsync<BookListResponse>("key")).ReturnsAsync((BookListResponse)null!);
        _bookRepository.Setup(r => r.GetFilterBooks(It.IsAny<PracticalWork.Library.Contracts.v1.Enums.BookStatus?>(),
            It.IsAny<BookCategory?>(), It.IsAny<string>())).ReturnsAsync(fromRepo);
        _pagination.Setup(p => p.PaginationBooks(fromRepo, 1, 10)).Returns(paginated);

        var result = await _sut.GetBooks(1, 10, null, null, null);

        result.Should().BeSameAs(paginated);
        _cache.Verify(c => c.SetAsync("key", paginated, It.IsAny<TimeSpan?>()), Times.Once);
    }
}
