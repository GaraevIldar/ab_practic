namespace PracticalWork.Library.Exceptions.Book;

public sealed class BookBorrowedException : AppException
{
    public BookBorrowedException()
        : base("Книга выдана читателю") { }
}
