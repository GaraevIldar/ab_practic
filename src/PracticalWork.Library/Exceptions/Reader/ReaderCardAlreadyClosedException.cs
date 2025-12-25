namespace PracticalWork.Library.Exceptions.Reader;

public sealed class ReaderCardAlreadyClosedException : AppException
{
    public ReaderCardAlreadyClosedException()
        : base("Карточка читателя уже закрыта") { }
}