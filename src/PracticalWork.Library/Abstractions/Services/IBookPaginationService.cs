using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Abstractions.Services;

public interface IBookPaginationService
{
    BookListResponse PaginationBooks(
        BookListResponse source,
        int pageNumber,
        int pageSize);
}