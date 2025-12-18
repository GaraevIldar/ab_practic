using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v1.Books.Request;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Exceptions;
using PracticalWork.Library.Models;
using StackExchange.Redis;

namespace PracticalWork.Library.Services;

public class ReaderService: IReaderService
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
            throw new ReaderServiceException("Ошибка создания карточки читателя",ex);
        }
    }
    public async Task<Guid> ExtendReaderCard(Guid id, ExtendReaderRequest request)
    {
        try
        {
            return await _repository.UpdateReaderExpiryDateAsync(id, request);
        }
        catch (Exception ex)
        {
            throw new ReaderServiceException("Ошибка при попытке продлить срок действия карточки читателя",ex);
        }
    }

    public async Task<CloseReaderCardResponse> CloseReaderCard(Guid id)
    {
        try
        {
            var readerId = await _repository.CloseReaderCard(id);
            return new CloseReaderCardResponse(readerId);
        }
        catch (Exception ex)
        {
            throw new ReaderServiceException("Ошибка при попытке закрыть карточку читателя",ex);
        }
    }

    public async Task<IList<Book>> GetBooksReaders(Guid readerId)
    {
        var readerExists = await _repository.IsReaderExist(readerId);

        if (!readerExists)
            throw new ReaderServiceException("Читатель не найден");
        try
        {
            return await _repository.GetReaderBooks(readerId);
        }
        catch (Exception ex)
        {
            throw new RedisException("Ошибка при получении книг пользователя", ex);
        }
    }
}