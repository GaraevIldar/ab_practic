using Microsoft.EntityFrameworkCore;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v1.Books.Request;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Data.PostgreSql.Entities;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Data.PostgreSql.Repositories;

public sealed class BookRepository : IBookRepository
{
    private readonly AppDbContext _appDbContext;

    public BookRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<Guid> CreateBook(Book book)
    {
        AbstractBookEntity entity = book.Category switch
        {
            BookCategory.ScientificBook => new ScientificBookEntity(),
            BookCategory.EducationalBook => new EducationalBookEntity(),
            BookCategory.FictionBook => new FictionBookEntity(),
            _ => throw new ArgumentException($"Неподдерживаемая категория книги: {book.Category}",
                nameof(book.Category))
        };

        entity.Title = book.Title;
        entity.Description = book.Description;
        entity.Year = book.Year;
        entity.Authors = book.Authors;
        entity.Status = book.Status;

        _appDbContext.Add(entity);
        await _appDbContext.SaveChangesAsync();

        return entity.Id;
    }

    public async Task<Guid> UpdateBook(Guid id, Book book)
    {
        var existingEntity = await FindBookById(id);
        if (existingEntity == null)
            throw new ArgumentException($"Книга с ID {id} не найдена");

        if (existingEntity.Status == BookStatus.Archived)
            throw new InvalidOperationException($"Нельзя изменять книгу с ID {id}, так как она находится в архиве");


        existingEntity.Title = book.Title;
        existingEntity.Description = book.Description;
        existingEntity.Year = book.Year;
        existingEntity.Authors = book.Authors;

        _appDbContext.Update(existingEntity);
        await _appDbContext.SaveChangesAsync();

        return existingEntity.Id;
    }

    private async Task<AbstractBookEntity> FindBookById(Guid id)
    {
        var scientificBook = await _appDbContext.ScientificBooks
            .FirstOrDefaultAsync(b => b.Id == id);
        if (scientificBook != null) return scientificBook;

        var fictionBook = await _appDbContext.FictionBooks
            .FirstOrDefaultAsync(b => b.Id == id);
        if (fictionBook != null) return fictionBook;

        var educationalBook = await _appDbContext.EducationalBooks
            .FirstOrDefaultAsync(b => b.Id == id);
        if (educationalBook != null) return educationalBook;

        return null;
    }

    public async Task<ArchiveBookResponse> MoveToArchive(Guid id)
    {
        var existingEntity = await FindBookById(id);
        if (existingEntity == null)
            throw new ArgumentException($"Книга с ID {id} не найдена");

        if (existingEntity.Status == BookStatus.Archived)
            throw new InvalidOperationException(
                $"Нельзя переместить книгу с ID {id}, так как она уже находится в архиве");


        existingEntity.Status = BookStatus.Archived;

        await _appDbContext.SaveChangesAsync();

        return new ArchiveBookResponse(
            existingEntity.Id,
            existingEntity.Title,
            DateTime.UtcNow
        );
    }

    public async Task<BookListResponse> GetBooks()
    {
        var books = await _appDbContext.Books
            .OrderBy(b => b.Status)
            .ToListAsync();

        var response = new BookListResponse()
        {
            TotalCount = books.Count,
            Books = books.Select(b => new BookItemResponse
            {
                Id = b.Id,
                Title = b.Title,
                Authors = b.Authors,
                Description = b.Description,
                Year = b.Year,
                Status = b.Status.ToString(),
                CoverImagePath = b.CoverImagePath,
                Category = b switch
                {
                    FictionBookEntity => (Contracts.v1.Enums.BookCategory)BookCategory.FictionBook,
                    EducationalBookEntity => (Contracts.v1.Enums.BookCategory)BookCategory.EducationalBook,
                    ScientificBookEntity => (Contracts.v1.Enums.BookCategory)BookCategory.ScientificBook,
                    _ => (Contracts.v1.Enums.BookCategory)BookCategory.Default
                }
            }).ToList()

        };
        

        return response;
    }
    public async Task<BookListResponse> GetBooksNoArchive()
    {
        var books = await _appDbContext.Books
            .Where(b=> b.Status != BookStatus.Archived)
            .OrderBy(b => b.Status)
            .ToListAsync();

        var response = new BookListResponse()
        {
            TotalCount = books.Count,
            Books = books.Select(b => new BookItemResponse
            {
                Id = b.Id,
                Title = b.Title,
                Authors = b.Authors,
                Description = b.Description,
                Year = b.Year,
                Status = b.Status.ToString(),
                CoverImagePath = b.CoverImagePath,
                Category = b switch
                {
                    FictionBookEntity => (Contracts.v1.Enums.BookCategory)BookCategory.FictionBook,
                    EducationalBookEntity => (Contracts.v1.Enums.BookCategory)BookCategory.EducationalBook,
                    ScientificBookEntity => (Contracts.v1.Enums.BookCategory)BookCategory.ScientificBook,
                    _ => (Contracts.v1.Enums.BookCategory)BookCategory.Default
                }
            }).ToList()

        };
        

        return response;
    }

    public async Task<bool> IsBookExist(Guid id)
    {
        var book = await _appDbContext.Books.FirstOrDefaultAsync(b => b.Id == id);
        
        if (book == null) return false;

        return true;
    }
    // public async Task<BookDetailsResponse> AddDetails(AddBookDetailsRequest details)
    // {
    //     var existingEntity = await FindBookByIdAsync(details.Id);
    //     return  
    // }
}