namespace PracticalWork.Library.Events;

/// <summary>
/// Событие создания новой книги в библиотеке
/// </summary>
/// <param name="BookId">Уникальный идентификатор книги</param>
/// <param name="Title">Название книги</param>
/// <param name="Category">Категория книги</param>
/// <param name="Authors">Массив авторов книги</param>
/// <param name="Year">Год издания</param>
public sealed record BookCreatedEvent(
    Guid BookId,
    string Title,
    string Category,
    string[] Authors,
    int Year
) : BaseLibraryEvent("book.created")
{
    public BookCreatedEvent(Guid bookId, string title, string category, string[] authors,
        int year, DateTime createdAt)
        : this(
            bookId,
            title,
            category,
            authors ?? Array.Empty<string>(),
            year
        )
    {
    }
}