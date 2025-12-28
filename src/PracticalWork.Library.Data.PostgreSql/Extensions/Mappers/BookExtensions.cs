using PracticalWork.Library.Data.PostgreSql.Entities;
using PracticalWork.Library.Models;
using PracticalWork.Library.Enums;

namespace PracticalWork.Library.Data.PostgreSql.Extensions.Mappers;

public static class BookExtensions
{
    public static Book ToBook(this AbstractBookEntity entity) =>
        new()
        {
            Id = entity.Id,
            Title = entity.Title,
            Authors = entity.Authors,
            Description = entity.Description,
            Year = entity.Year,
            Status = entity.Status,
            CoverImagePath = entity.CoverImagePath,
        };
    public static Borrow ToBookBorrow(this BookBorrowEntity entity) =>
        new()
        {
            Id = entity.Id,
            Status = entity.Status,
            ReturnDate = entity.ReturnDate,
            DueDate = entity.DueDate,
            BorrowDate = entity.BorrowDate,
        };
}