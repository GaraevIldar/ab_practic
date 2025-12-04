using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Data.PostgreSql.Entities;
using PracticalWork.Library.Data.PostgreSql.Extensions.Mappers;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Exceptions;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Data.PostgreSql.Repositories;

public class LibraryRepository: ILibraryRepository
{
    private readonly AppDbContext _appDbContext;

    public LibraryRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    public async Task<Borrow> GetBookBorrow(Guid bookId)
    {
        var borrow = await _appDbContext.BookBorrows
            .SingleOrDefaultAsync(b => b.BookId == bookId);
        return borrow.ToBookBorrow();
    }
    public async Task<BorrowBookResponse> BorrowBook(Guid bookId, Guid readerId)
    {
        
        var book = await _appDbContext.Books.FirstOrDefaultAsync(b => b.Id == bookId);
        
        var reader = await _appDbContext.Readers.FirstOrDefaultAsync(r => r.Id == readerId);

        var borrow = new BookBorrowEntity()
        {
            BookId = book.Id,
            ReaderId = reader.Id,
            BorrowDate = DateOnly.FromDateTime(DateTime.Now),
            DueDate = DateOnly.FromDateTime(DateTime.Now).AddDays(30),
            Status = BookIssueStatus.Issued,
        };
        
        book.Status = BookStatus.Borrow;
        
        _appDbContext.Books.Update(book);
        _appDbContext.BookBorrows.Add(borrow);
        
        await _appDbContext.SaveChangesAsync();

        return new BorrowBookResponse(borrow.Id);
    }

    public async Task<ReturnBookResponse> ReturnBook(Guid bookId)
    {
        var book = await _appDbContext.BookBorrows.FirstOrDefaultAsync(b => b.Id == bookId);
        

        book.Status = BookIssueStatus.Returned;
        book.ReturnDate = DateOnly.FromDateTime(DateTime.Now);
        
        await _appDbContext.SaveChangesAsync();
        
        return new ReturnBookResponse(book.Id);
    }
    
}