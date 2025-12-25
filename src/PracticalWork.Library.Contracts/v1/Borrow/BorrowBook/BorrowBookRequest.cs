namespace PracticalWork.Library.Contracts.v1.Books.Request;

/// <summary>
/// Запрос на выдачу книги
/// </summary>
/// <param name="IdReader"> Id читателя</param>
/// <param name="IdBook">Id книги</param>
public sealed record BorrowBookRequest(Guid IdReader, Guid IdBook);