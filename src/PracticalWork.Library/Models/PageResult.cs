namespace PracticalWork.Library.Models;

public class PageResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int TotalItems { get; }
    public int PageNumber { get; }
    public int PageSize { get; }

    public int TotalPages => 
        (int)Math.Ceiling(TotalItems / (double)PageSize);

    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;

    public PageResult(
        IReadOnlyList<T> items,
        int totalItems,
        int pageNumber,
        int pageSize)
    {
        Items = items;
        TotalItems = totalItems;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
