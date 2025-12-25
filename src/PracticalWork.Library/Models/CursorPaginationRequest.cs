using System.Text;

namespace PracticalWork.Library.Models;

/// <summary>
/// Запрос на пагинацию
/// </summary>
public class CursorPaginationRequest
{
    /// <summary>
    /// Cтрока хеширования
    /// </summary>
    public string Cursor { get; set; }
    /// <summary>
    /// Размер страницы
    /// </summary>
    public required int PageSize { get; set; }
    /// <summary>
    /// Направление пагинации
    /// </summary>
    public required bool Forward { get; set; }
    /// <summary>
    /// декодировать курсор из строки
    /// </summary>
    /// <returns>объект курсора</returns>
    public Cursor DecodeCursor()
    {
        var stringCursor = Encoding.UTF8.GetString(Convert.FromBase64String(Cursor));
        return new Cursor
        {
            Id = Guid.Parse(stringCursor)
        };
    }
}