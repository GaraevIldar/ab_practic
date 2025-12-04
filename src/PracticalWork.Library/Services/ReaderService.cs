using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v1.Books.Request;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Exceptions;
using PracticalWork.Library.Models;

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
            return await _repository.CloseReaderCard(id);
        }
        catch (Exception ex)
        {
            throw new ReaderServiceException("Ошибка при попытке закрыть карточку читателя",ex);
        }
    }
}