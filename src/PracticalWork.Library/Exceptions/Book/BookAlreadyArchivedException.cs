namespace PracticalWork.Library.Exceptions.Book;

public sealed class BookAlreadyArchivedException : AppException
{
    public BookAlreadyArchivedException()
        : base("Книга уже находится в архиве") { }
}