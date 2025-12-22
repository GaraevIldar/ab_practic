using Microsoft.EntityFrameworkCore;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v1.Books.Request;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Data.PostgreSql.Entities;
using PracticalWork.Library.Data.PostgreSql.Extensions.Mappers;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Data.PostgreSql.Repositories;

public class ReaderRepository: IReaderRepository
{
    private readonly AppDbContext _dbContext;

    public ReaderRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<Guid> CreateReader(Reader reader)
    {
        var existingReader = await _dbContext.Readers
            .FirstOrDefaultAsync(r => r.PhoneNumber == reader.PhoneNumber);
    
        if (existingReader != null)
        {
            throw new InvalidOperationException($"Читатель с номером телефона {reader.PhoneNumber} уже существует");
        }
        ReaderEntity entity = new ReaderEntity();
        
        entity.FullName = reader.FullName;
        entity.PhoneNumber = reader.PhoneNumber;
        entity.ExpiryDate = reader.ExpiryDate;
        entity.IsActive = true;
        
        _dbContext.Readers.Add(entity);
        await _dbContext.SaveChangesAsync();

        return entity.Id;
    }

    public async Task<Guid> UpdateReaderExpiryDateAsync(Guid id, ExtendReaderRequest request)
    {
        var reader = await _dbContext.Readers
            .FirstOrDefaultAsync(r => r.Id == id);

        reader.ExpiryDate = request.NewExpiryDate;
        
        await _dbContext.SaveChangesAsync();
        
        return reader.Id;
    }

    public async Task<bool> IsReaderExist(Guid id)
    {
        var reader = await _dbContext.Readers
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
        
        if (reader == null)
            return false;
        
        return true;
    }

    public async Task<bool> IsBookBorrowsExist(Guid id)
    {
        var reader = await _dbContext.Readers.FirstOrDefaultAsync(r => r.Id == id);

        var bookBorrows = await _dbContext.BookBorrows
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.ReaderId == reader.Id && b.Status == BookIssueStatus.Issued);
        
       return bookBorrows != null;
    }

    public async Task<List<BookBorrowEntity>> GetBookBorrows(Guid id)
    {
        var reader = await _dbContext.Readers.FirstOrDefaultAsync(r => r.Id == id);
        
        var bookBorrows = await _dbContext.BookBorrows
            .AsNoTracking()
            .Where(b => b.ReaderId == reader.Id && b.Status == BookIssueStatus.Issued)
            .ToListAsync();
        
        return bookBorrows;
    }
    public async Task<Guid> CloseReaderCard(Guid id)
    {
        var reader = await _dbContext.Readers.FirstOrDefaultAsync(r => r.Id == id);
        
        reader.IsActive = false;
        reader.ExpiryDate = DateOnly.FromDateTime(DateTime.Now);
        
        await _dbContext.SaveChangesAsync();

        return id;
    }
    public async Task<string> GetBookNonReturners(Guid id)
    {
        var bookBorrows = await GetBookBorrows(id);
        
        var bookList = bookBorrows
            .Select(b => $"ID книги: {b.BookId}, Дата взятия: {b.BorrowDate:dd.MM.yyyy}")
            .ToList();
        return ($"Читатель с id {id} вернул не все книги:\n"
                                                + string.Join("\n", bookList));
    }
}