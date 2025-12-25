namespace PracticalWork.Library.Exceptions.Book;

public sealed class BookArchivedException : AppException
{
    public BookArchivedException(Guid id)
        : base($"Книга с ID {id} находится в архиве") { }
}
