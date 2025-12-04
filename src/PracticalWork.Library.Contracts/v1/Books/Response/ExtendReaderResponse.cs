namespace PracticalWork.Library.Contracts.v1.Books.Response;

/// <summary>
/// Ответ на обновление карточки читателя  
/// </summary>
/// <param name="Id">Идентификатор читателя</param>
public sealed record ExtendReaderResponse(Guid Id);