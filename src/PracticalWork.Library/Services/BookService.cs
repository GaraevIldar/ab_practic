using Microsoft.AspNetCore.Http;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Data.Minio;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Exceptions;
using PracticalWork.Library.Exceptions.Book;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Services;

public sealed class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly ICacheService _cacheService;
    private readonly IMinioService _minioService;
    private const string BooksCacheVersionKey = "books:cache:version";
    private const int PageCacheDurationMinutes = 10;

    public BookService(IBookRepository bookRepository,
        ICacheService cacheService,
        IMinioService minioService)
    {
        _bookRepository = bookRepository;
        _cacheService = cacheService;
        _minioService = minioService;
    }

    public async Task<Guid> CreateBook(Book book)
    {
        book.Status = BookStatus.Available;
        
        var id = await _bookRepository.CreateBook(book);
            
        await IncreaseCacheVersion();

        return id;
    }

    public async Task UpdateBook(Guid id, Book book)
    {
        var existingEntity = await _bookRepository.GetBookById(id);
        
        if (existingEntity == null)
            throw new BookNotFoundException(id);

        if (existingEntity.Status == BookStatus.Archived)
            throw new BookArchivedException(id);
        
        await _bookRepository.UpdateBook(id, book);
        await IncreaseCacheVersion();
    }
    
    public async Task MoveToArchive(Guid id)
    {
        var book = await _bookRepository.GetBookById(id);
        
        if (book == null)
            throw new BookNotFoundException(id);

        if (book.Status == BookStatus.Archived)
            throw new BookAlreadyArchivedException();
        
        if (book.Status == BookStatus.Borrow)
            throw new BookBorrowedException();
            
        await _bookRepository.MoveToArchive(id);
        await IncreaseCacheVersion();
    }
    
    public async Task<BookListResponse> GetBooks()
    {
        var cacheKey = await BuildBooksCacheKey();

        var cached = await _cacheService.GetAsync<BookListResponse>(cacheKey);
        if (cached != null)
            return cached;

        var books = await _bookRepository.GetBooks();

        await _cacheService.SetAsync(
            cacheKey,
            books,
            TimeSpan.FromMinutes(PageCacheDurationMinutes));

        return books;
    }
    public async Task AddBookDetails(Guid bookId, string description, IFormFile coverFile)
    {
        var book = await _bookRepository.GetBookById(bookId);

        if (book == null)
            throw new BookNotFoundException(bookId);

        var ext = Path.GetExtension(coverFile.FileName).ToLower();
        var now = DateTime.UtcNow;

        string objectName = $"book-covers/{now:yyyy}/{now:MM}/{bookId}{ext}";

        using var stream = coverFile.OpenReadStream();
        var path = await _minioService.UploadFileAsync(
            objectName,
            stream,
            coverFile.ContentType);

        book.Description = description;
        book.CoverImagePath = path;

        await _bookRepository.UpdateBook(bookId, book);
        await IncreaseCacheVersion();
    }

    private async Task<int> GetCacheVersion()
    {
        var version = await _cacheService.GetAsync<int?>(BooksCacheVersionKey);
        return version ?? 1;
    }

    private async Task IncreaseCacheVersion()
    {
        var version = await GetCacheVersion();
        await _cacheService.SetAsync(BooksCacheVersionKey, version + 1);
    }

    private async Task<string> BuildBooksCacheKey()
    {
        var version = await GetCacheVersion();
        return $"books:list:v{version}";
    }
}