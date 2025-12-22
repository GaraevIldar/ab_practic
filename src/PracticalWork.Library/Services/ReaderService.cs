using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v1.Books.Request;
using PracticalWork.Library.Exceptions;
using PracticalWork.Library.Models;

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
        var readerExists = await _repository.IsReaderExist(id);

        if (!readerExists)
            throw new ReaderServiceException($"Читатель с id {id} не существует");
        try
        {
            return await _repository.UpdateReaderExpiryDateAsync(id, request);
        }
        catch (Exception ex)
        {
            throw new ReaderServiceException("Ошибка при попытке продлить срок действия карточки читателя", ex);
        }
    }

    public async Task<string> CloseReaderCard(Guid id)
    {
        var readerExists = await _repository.IsReaderExist(id);

        if (!readerExists)
            throw new ReaderServiceException($"Читатель с id {id} не существует");

        var borrowBooksExists = await _repository.IsBookBorrowsExist(id);

        try
        {
            if (!borrowBooksExists)
                return _repository.CloseReaderCard(id).ToString();
           
            return await _repository.GetBookNonReturners(id);
        }
        catch (Exception ex)
        {
            throw new ReaderServiceException("Ошибка при попытке закрыть карточку читателя", ex);
        }
    }
}