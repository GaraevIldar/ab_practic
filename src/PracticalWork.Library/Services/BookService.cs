using Microsoft.AspNetCore.Http;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Data.Minio;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Exceptions;
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
        try
        {
            var id = await _bookRepository.CreateBook(book);
            
            await IncreaseCacheVersion();

            return id;
        }
        catch (Exception ex)
        {
            throw new BookServiceException("Ошибка создание книги!", ex);
        }
    }

    public async Task UpdateBook(Guid id, Book book)
    {
        var existingEntity = await _bookRepository.GetBookById(id);
        
        if (existingEntity == null)
            throw new BookServiceException($"Книга с ID {id} не найдена");

        if (existingEntity.Status == BookStatus.Archived)
            throw new BookServiceException(
                $"Нельзя изменять книгу с ID {id}, так как она находится в архиве");
        
        try
        {
            await _bookRepository.UpdateBook(id, book);
            
            await IncreaseCacheVersion();
        }
        catch (Exception ex)
        {
            throw new BookServiceException("Ошибка редактирования книги", ex);
        }
    }
    
    public async Task MoveToArchive(Guid id)
    {
        var existingEntity = await _bookRepository.GetBookById(id);
        
        if (existingEntity == null)
            throw new BookServiceException($"Книга с ID {id} не найдена");
        try
        { 
            await _bookRepository.MoveToArchive(id);
            
            await IncreaseCacheVersion();
        }
        catch (Exception ex)
        {
            throw new BookServiceException("Ошибка перемещения книги в архив", ex);
        }
    }
    
    public async Task<BookListResponse> GetBooks()
    {
        try
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
        catch (Exception ex)
        {
            throw new BookServiceException("Ошибка получения книг", ex);
        }
    }
    public async Task AddBookDetails(Guid bookId, string description, IFormFile coverFile)
    {
        var book = await _bookRepository.GetBookById(bookId);
        if (book == null)
            throw new BookServiceException($"Книга с ID {bookId} не найдена");
        
        var ext = Path.GetExtension(coverFile.FileName).ToLower();
        
        var now = DateTime.UtcNow;
        string objectName = $"book-covers/{now:yyyy}/{now:MM}/{bookId}{ext}";

        string path;
        using (var stream = coverFile.OpenReadStream())
        {
            path = await _minioService.UploadFileAsync(objectName, stream, coverFile.ContentType);
        }
        
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