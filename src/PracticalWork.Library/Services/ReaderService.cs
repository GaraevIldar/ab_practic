using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v1.Books.Request;
using PracticalWork.Library.Exceptions;
using PracticalWork.Library.Exceptions.Reader;
using PracticalWork.Library.Models;
using StackExchange.Redis;

namespace PracticalWork.Library.Services;

public class ReaderService : IReaderService
{
    private readonly IReaderRepository _repository;

    public ReaderService(IReaderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> CreateReader(Reader reader)
    {
        try
        {
            return await _repository.CreateReader(reader);
        }
        catch (Exception ex)
        {
            throw new ReaderServiceException("Ошибка создания карточки читателя", ex);
        }
    }

    public async Task<Guid> ExtendReaderCard(Guid id, ExtendReaderRequest request)
    {
        if (!await _repository.IsReaderExist(id))
            throw new ReaderNotFoundException(id);

        try
        {
            return await _repository.UpdateReaderExpiryDateAsync(id, request);
        }
        catch (Exception ex)
        {
            throw new ReaderServiceException(
                "Ошибка при попытке продлить срок действия карточки читателя", ex);
        }
    }

    public async Task<string> CloseReaderCard(Guid id)
    {
        if (!await _repository.IsReaderExist(id))
            throw new ReaderNotFoundException(id);

        if (await _repository.IsBookBorrowsExist(id))
            throw new ReaderHasBorrowedBooksException(await _repository.GetBookNonReturners(id));

        try
        {
            return (await _repository.CloseReaderCard(id)).ToString();
        }
        catch (Exception ex)
        {
            throw new ReaderServiceException("Ошибка при попытке закрыть карточку читателя", ex);
        }
    }

    public async Task<IList<Book>> GetBooksReaders(Guid readerId)
    {
        if (!await _repository.IsReaderExist(readerId))
            throw new ReaderNotFoundException(readerId);

        try
        {
            return await _repository.GetReaderBooks(readerId);
        }
        catch (Exception ex)
        {
            throw new ReaderServiceException(
                "Ошибка при получении книг читателя", ex);
        }
    }
}