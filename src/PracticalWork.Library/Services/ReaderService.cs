using Microsoft.Extensions.Options;
using PracticalWork.Library.Abstractions.MessageBroker;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Configuration;
using PracticalWork.Library.Contracts.v1.Books.Request;
using PracticalWork.Library.Events;
using PracticalWork.Library.Exceptions;
using PracticalWork.Library.Exceptions.Reader;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Services;

public class ReaderService : IReaderService
{
    private readonly IReaderRepository _repository;
    private readonly IRabbitPublisher _publisher;
    private readonly LibraryRabbitConfig _rabbit;
    private readonly TimeProvider _timeProvider;

    public ReaderService(
        IReaderRepository repository,
        IRabbitPublisher publisher,
        IOptions<LibraryRabbitConfig> rabbitConfig,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _publisher = publisher;
        _rabbit = rabbitConfig.Value;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> CreateReader(Reader reader)
    {
        try
        {
            var idReader = await _repository.CreateReader(reader);
            var message = new ReaderCreatedEvent(idReader, reader.FullName,
                reader.PhoneNumber, reader.ExpiryDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
            await _publisher.PublishAsync(
                _rabbit.ExchangeName, 
                _rabbit.ReaderCreate.RoutingKey, 
                message);
            //добавить кэш
            return idReader;
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
            var readerFullName = await _repository.GetReaderFullNameById(id);
            var message = new ReaderClosedEvent(id, readerFullName, _timeProvider.GetUtcNow().UtcDateTime);
            await _publisher.PublishAsync(
                _rabbit.ExchangeName, 
                _rabbit.ReaderClose.RoutingKey, 
                message);
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