using PracticalWork.Library.Contracts.v1.Abstracts;

namespace PracticalWork.Library.Contracts.v1.Books.Request;

/// <summary>
/// Запрос на обновление книги
/// </summary>
/// <param name="Title">Название книги</param>
/// <param name="Authors">Список авторов</param>
/// <param name="Description">Описание книги</param>
/// <param name="Year">Год издания книги</param>
public sealed record UpdateBookRequest(string Title, IReadOnlyList<string> Authors, string Description, int Year)
    : AbstractBook(Title, Authors, Description, Year);