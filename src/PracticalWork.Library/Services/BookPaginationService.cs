using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Services;

public class BookPaginationService : IBookPaginationService
{
    public BookListResponse PaginationBooks(BookListResponse source, int pageNumber, int pageSize)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (pageNumber < 1)
            pageNumber = 1;

        if (pageSize < 1)
            pageSize = 10;

        var totalCount = source.TotalCount;

        var pagedBooks = source.Books
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new BookListResponse
        {
            Books = pagedBooks,
            TotalCount = totalCount
        };
    }
}
