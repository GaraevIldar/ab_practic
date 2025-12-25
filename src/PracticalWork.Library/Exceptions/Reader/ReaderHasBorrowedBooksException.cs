namespace PracticalWork.Library.Exceptions.Reader;

public sealed class ReaderHasBorrowedBooksException : AppException
{
    public ReaderHasBorrowedBooksException(string books)
        : base("Нельзя закрыть карточку читателя, пока есть невозвращённые книги\n"+
               books) { }
}