using PracticalWork.Library.Contracts.v1.Enums;

namespace PracticalWork.Library.Contracts.v1.Books.Response;

public sealed class BookListResponse
{
    /// <summary>Список книг</summary>
    public IReadOnlyList<BookItemResponse> Books { get; set; }

    /// <summary>Общее количество найденных книг (без учёта пагинации)</summary>
    public int TotalCount { get; set; }
}
public class BookItemResponse
{
    /// <summary>Идентификатор книги</summary>
    public Guid Id { get; set; }

    /// <summary>Название книги</summary>
    public string Title { get; set; }

    /// <summary>Авторы</summary>
    public IReadOnlyList<string> Authors { get; set; }

    /// <summary>Краткое описание</summary>
    public string Description { get; set; }

    /// <summary>Год издания</summary>
    public int Year { get; set; }

    /// <summary>Статус книги</summary>
    public string Status { get; set; }

    /// <summary>Путь к обложке</summary>
    public string CoverImagePath { get; set; }

    /// <summary>Тип книги (Учебная / Художественная / Научная)</summary>
    public BookCategory Category { get; set; }
}