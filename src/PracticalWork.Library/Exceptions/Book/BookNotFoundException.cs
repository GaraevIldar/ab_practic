namespace PracticalWork.Library.Exceptions.Book;

public sealed class BookNotFoundException : AppException
{
    public BookNotFoundException(Guid id)
        : base($"Книга с ID {id} не найдена") { }
}
