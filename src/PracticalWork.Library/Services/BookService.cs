﻿using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v1.Books.Request;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Exceptions;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Services;

public sealed class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<Guid> CreateBook(Book book)
    {
        book.Status = BookStatus.Available;
        try
        {
            return await _bookRepository.CreateBook(book);
        }
        catch (Exception ex)
        {
            throw new BookServiceException("Ошибка создание книги!", ex);
        }
    }

    public async Task<Guid> UpdateBook(Guid id, Book book)
    {
        try
        {
            return await _bookRepository.UpdateBook(id, book);
        }
        catch (Exception ex)
        {
            throw new BookServiceException("Ошибка редактирования книги", ex);
        }
    }
    
    public async Task<ArchiveBookResponse> MoveToArchive(Guid id)
    {
        try
        {
            return await _bookRepository.MoveToArchive(id);
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
            return await _bookRepository.GetBooks();
        }
        catch (Exception ex)
        {
            throw new BookServiceException("Ошибка перемещения книги в архив", ex);
        }
    }
    
    // public async Task<BookListResponse> AddDetails(AddBookDetailsRequest details)
    // {
    //     try
    //     {
    //         return await _bookRepository.AddDetails(details);
    //     }
    //     catch (Exception ex)
    //     {
    //         throw new BookServiceException("Ошибка перемещения книги в архив", ex);
    //     }
    // }
}