using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Data.PostgreSql.Entities;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Exceptions;

namespace PracticalWork.Library.Data.PostgreSql.Repositories;

public class LibraryRepository: ILibraryRepository
{
    private readonly AppDbContext _appDbContext;
    private readonly IReaderRepository _readerRepository;
    private readonly IBookRepository _bookRepository;

    public LibraryRepository(AppDbContext appDbContext,
        IReaderRepository readerRepository, 
        IBookRepository bookRepository)
    {
        _appDbContext = appDbContext;
        _readerRepository = readerRepository;
        _bookRepository = bookRepository;
    }
    
    public async Task<BorrowBookResponse> BorrowBook(Guid bookId, Guid readerId)
    {
        var readerExists = await _readerRepository.IsReaderExist(readerId);
        var bookExists = await _bookRepository.IsBookExist(bookId);

        if (!readerExists || !bookExists)
            throw new InvalidOperationException("Читатель или книга не найдены");
        
        
        var book = await _appDbContext.Books.FirstOrDefaultAsync(b => b.Id == bookId);
        
        var reader = await _appDbContext.Readers.FirstOrDefaultAsync(r => r.Id == readerId);

        if (book.Status != BookStatus.Available) 
            throw new InvalidOperationException("Книга находится в архиве или уже выдана");

        if (!reader.IsActive)
            throw new InvalidOperationException("Карточка читателя неактивна");

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

        if (book == null)
            throw new InvalidOperationException("Нет записи о выдачи");

        book.Status = BookIssueStatus.Returned;
        book.ReturnDate = DateOnly.FromDateTime(DateTime.Now);
        
        await _appDbContext.SaveChangesAsync();
        
        return new ReturnBookResponse(book.Id);
    }
}