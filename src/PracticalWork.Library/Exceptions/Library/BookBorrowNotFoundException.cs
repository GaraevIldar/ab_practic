namespace PracticalWork.Library.Exceptions.Library;

public sealed class BookBorrowNotFoundException : AppException
{
    public BookBorrowNotFoundException(Guid bookId)
        : base($"Нет записи о выдаче книги с ID {bookId}") { }
}