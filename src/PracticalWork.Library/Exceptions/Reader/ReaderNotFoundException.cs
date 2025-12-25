namespace PracticalWork.Library.Exceptions.Reader;

public sealed class ReaderNotFoundException : AppException
{
    public ReaderNotFoundException(Guid id)
        : base($"Читатель с ID {id} не найден") { }
    public ReaderNotFoundException()
        : base($"Читатель не найден") { }
}