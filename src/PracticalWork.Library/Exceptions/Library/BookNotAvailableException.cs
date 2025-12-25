namespace PracticalWork.Library.Exceptions.Library;

public sealed class BookNotAvailableException : AppException
{
    public BookNotAvailableException()
        : base("Книга недоступна для выдачи") { }
}