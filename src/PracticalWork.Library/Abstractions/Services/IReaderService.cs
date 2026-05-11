using PracticalWork.Library.Contracts.v1.Books.Request;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Abstractions.Services;

/// <summary>
/// Сервис управления карточками читателей
/// </summary>
public interface IReaderService
{
    /// <summary>
    /// Создание карточки читателя
    /// </summary>
    Task<Guid> CreateReader(Reader reader);

    /// <summary>
    /// Продление срока действия карточки читателя
    /// </summary>
    Task<Guid> ExtendReaderCard(Guid id, ExtendReaderRequest request);

    /// <summary>
    /// Закрытие карточки читателя
    /// </summary>
    Task<string> CloseReaderCard(Guid id);

    /// <summary>
    /// Получение списка книг, выданных читателю
    /// </summary>
    Task<IList<Book>> GetBooksReaders(Guid id);
}