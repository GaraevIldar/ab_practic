namespace PracticalWork.Library.Models;

/// <summary>
/// Ответ пагинации
/// </summary>
/// <typeparam name="T">тип объектов пагинации</typeparam>
public class CursorPaginationResponse<T>
{
    /// <summary>
    /// Элементы страницы
    /// </summary>
    public required IReadOnlyList<T> Items { get; set; }
    /// <summary>
    ///  Курсор следующей записи
    /// </summary>
    public string NextCursor { get; set; }
    /// <summary>
    /// Курсор предыдущей записи
    /// </summary>
    public string PreviousCursor { get; set; }
    /// <summary>
    /// Наличие следующей записи
    /// </summary>
    public bool HasNext { get; set; }
    /// <summary>
    /// Наличие предыдущей записи
    /// </summary>
    public bool HasPrevious { get; set; }
}