namespace PracticalWork.Library.Contracts.v1.Books.Request;

/// <summary>
/// Запрос на возвращение книги 
/// </summary>
/// <param name="Id">Идентификатор книги</param>
public sealed record ReturnBookRequest(Guid Id);