using Microsoft.Extensions.Configuration;
using PracticalWork.Library.Abstractions.MessageBroker;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v1.Books.Request;
using PracticalWork.Library.Events;
using PracticalWork.Library.Exceptions;
using PracticalWork.Library.Exceptions.Reader;
using PracticalWork.Library.Models;
using StackExchange.Redis;

namespace PracticalWork.Library.Services;

public class ReaderService : IReaderService
{
    private readonly IReaderRepository _repository;
    private readonly IRabbitPublisher _publisher;
    private readonly IConfigurationSection _rabbitLibrarySection;
    private readonly string _exchangeName;

    public ReaderService(
        IReaderRepository repository,
        IRabbitPublisher publisher,
        IConfiguration configuration)
    {
        _repository = repository;
        _publisher = publisher;
        _rabbitLibrarySection = configuration.GetSection("App:RabbitMQ:Library");
        _exchangeName = _rabbitLibrarySection["ExchangeName"];
        _publisher = publisher;
    }

    public async Task<Guid> CreateReader(Reader reader)
    {
        try
        {
            var idReader = await _repository.CreateReader(reader); 
            var message = new ReaderCreatedEvent(idReader, reader.FullName,
                reader.PhoneNumber, DateTime.UtcNow);
            await _publisher.PublishAsync(
                _exchangeName, 
                _rabbitLibrarySection["ReaderCreate:RoutingKey"], 
                message);
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
            var message = new ReaderClosedEvent(id, readerFullName,
                DateTime.UtcNow, "Вызван метод закрытия карточки");
            await _publisher.PublishAsync(
                _exchangeName, 
                _rabbitLibrarySection["CloseReader:RoutingKey"], 
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