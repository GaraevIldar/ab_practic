using Microsoft.EntityFrameworkCore;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v1.Books.Request;
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
            _ => throw new ArgumentException($"Неподдерживаемая категория книги: {book.Category}", nameof(book.Category))
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
        var existingEntity = await FindBookByIdAsync(id);
        if (existingEntity == null)
            throw new ArgumentException($"Книга с ID {id} не найдена");
        
        if(existingEntity.Status == BookStatus.Archived)
            throw new InvalidOperationException($"Нельзя изменять книгу с ID {id}, так как она находится в архиве");
            
        
        existingEntity.Title = book.Title;
        existingEntity.Description = book.Description;
        existingEntity.Year = book.Year;
        existingEntity.Authors = book.Authors;
        
        _appDbContext.Update(existingEntity);
        await _appDbContext.SaveChangesAsync();
        
        return existingEntity.Id;
    }

    private async Task<AbstractBookEntity> FindBookByIdAsync(Guid id)
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

    private bool IsCategoryCompatible(AbstractBookEntity entity, BookCategory category)
    {
        return (entity is ScientificBookEntity && category == BookCategory.ScientificBook) ||
               (entity is FictionBookEntity && category == BookCategory.FictionBook) ||
               (entity is EducationalBookEntity && category == BookCategory.EducationalBook);
    }
    public async Task<Guid> MoveToArchive(Guid id)
    {
        var existingEntity = await FindBookByIdAsync(id);
        if (existingEntity == null)
            throw new ArgumentException($"Книга с ID {id} не найдена");
        
        if(existingEntity.Status == BookStatus.Archived)
            throw new InvalidOperationException($"Нельзя переместить книгу с ID {id}, так как она уже находится в архиве"); 
        

        existingEntity.Status = BookStatus.Archived;
        
        await _appDbContext.SaveChangesAsync();
        
        return existingEntity.Id;
    }
    
}