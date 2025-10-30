using PracticalWork.Library.Contracts.v1.Books.Request;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Data.PostgreSql.Entities;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Controllers.Mappers.v1;

public static class BooksExtensions
{
    public static Book ToBook(this CreateBookRequest request) =>
        new()
        {
            Authors = request.Authors,
            Title = request.Title,
            Description = request.Description,
            Year = request.Year,
            Category = (BookCategory)request.Category
        };

    public static Book ToBook(this UpdateBookRequest request) =>
        new()
        {
            Authors = request.Authors,
            Title = request.Title,
            Description = request.Description,
            Year = request.Year,
        };
}